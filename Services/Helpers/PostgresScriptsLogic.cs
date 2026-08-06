using AxiDataPackages.References;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AxiDataPackages.Services.Helpers
{
    public class PostgresScriptsLogic
    {
        private readonly AxDBService _axDBService;

        private readonly ILogger<PostgresScriptsLogic> _logger;

        public PostgresScriptsLogic
        (
            AxDBService axDBService,
            ILogger<PostgresScriptsLogic> logger
        )
        {
            _axDBService = axDBService;

            _logger = logger;
        }

        public async Task<(bool success, string data)> ExportFunctionAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportFunctionAsync Started : {name}", name);

                string sql = @"
                    SELECT pg_get_functiondef(p.oid) || ';' AS ddl
                    FROM pg_proc p
                    JOIN pg_namespace n ON n.oid = p.pronamespace
                    WHERE n.nspname = @schemaName
                    AND p.prokind = 'f'
                    AND p.proname = @name;
                ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportFunctionAsync Failed");
                throw;
            }
        }

        public async Task<(bool success, string data)> ExportProcedureAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportProcedureAsync Started : {name}", name);

                string sql = @"
                    SELECT pg_get_functiondef(p.oid) || ';' AS ddl
                    FROM pg_proc p
                    JOIN pg_namespace n ON n.oid = p.pronamespace
                    WHERE n.nspname = @schemaName
                    AND p.prokind = 'p'
                    AND p.proname = @name;
                ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportProcedureAsync Failed");

                throw;
            }
        }

        public async Task<(bool success, string data)> ExportViewAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportViewAsync Started : {name}", name);

                string sql = @"
                    SELECT
                        'CREATE OR REPLACE VIEW '
                        || quote_ident(schemaname) || '.' || quote_ident(viewname)
                        || ' AS ' || definition || ';' AS ddl
                    FROM pg_views
                    WHERE schemaname = @schemaName
                    AND viewname = @name;
                ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportViewAsync Failed");

                throw;
            }
        }

        public async Task<(bool success, string data)> ExportMtViewAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportMtViewAsync Started : {name}", name);

                string sql = @"
WITH materialized_views AS
(
    SELECT
        c.oid AS matview_oid,
        n.nspname AS schema_name,
        c.relname AS matview_name,

        'DROP MATERIALIZED VIEW IF EXISTS ' ||
        quote_ident(n.nspname) || '.' ||
        quote_ident(c.relname) AS drop_ddl,

        'CREATE MATERIALIZED VIEW ' ||
        quote_ident(n.nspname) || '.' ||
        quote_ident(c.relname) ||

        CASE
            WHEN ts.spcname IS NOT NULL
            THEN E'\nTABLESPACE ' || quote_ident(ts.spcname)
            ELSE ''
        END ||

        E'\nAS\n' ||
        regexp_replace(
            pg_get_viewdef(c.oid, false),
            ';[[:space:]]*$',
            ''
        ) ||

        CASE
            WHEN c.relispopulated
            THEN E'\nWITH DATA'
            ELSE E'\nWITH NO DATA'
        END AS create_ddl

    FROM pg_class c

    JOIN pg_namespace n
        ON n.oid = c.relnamespace

    LEFT JOIN pg_tablespace ts
        ON ts.oid = c.reltablespace

    WHERE c.relkind = 'm'
      AND n.nspname = @schemaName
      AND c.relname = @name
),

materialized_view_indexes AS
(
    SELECT
        i.indrelid AS matview_oid,
        index_class.relname AS index_name,

        CASE
            WHEN i.indisunique
            THEN 'UNIQUE INDEX'
            ELSE 'INDEX'
        END AS script_type,

        CASE
            WHEN i.indisunique
            THEN 3
            ELSE 4
        END AS sort_order,

        regexp_replace(
            pg_get_indexdef(i.indexrelid),
            ';[[:space:]]*$',
            ''
        ) AS index_ddl

    FROM pg_index i

    JOIN pg_class index_class
        ON index_class.oid = i.indexrelid

    JOIN materialized_views mv
        ON mv.matview_oid = i.indrelid
),

export_scripts AS
(
    /*
      Materialized-view DROP statement
    */
    SELECT
        1 AS sort_order,
        mv.schema_name,
        mv.matview_name,
        mv.matview_name AS object_name,
        'DROP' AS script_type,
        mv.drop_ddl AS ddl

    FROM materialized_views mv

    UNION ALL

    /*
      Materialized-view CREATE statement
    */
    SELECT
        2 AS sort_order,
        mv.schema_name,
        mv.matview_name,
        mv.matview_name AS object_name,
        'CREATE' AS script_type,
        mv.create_ddl AS ddl

    FROM materialized_views mv

    UNION ALL

    /*
      One separate row for every index
    */
    SELECT
        idx.sort_order,
        mv.schema_name,
        mv.matview_name,
        idx.index_name AS object_name,
        idx.script_type,
        idx.index_ddl AS ddl

    FROM materialized_view_indexes idx

    JOIN materialized_views mv
        ON mv.matview_oid = idx.matview_oid
)

SELECT
    schema_name,
    matview_name,
    object_name,
    script_type,
    ddl

FROM export_scripts

ORDER BY
    matview_name,
    sort_order,
    object_name
";

                /*string sql = @"
WITH materialized_views AS
(
    SELECT
        c.oid AS matview_oid,
        n.nspname AS schema_name,
        c.relname AS matview_name,

        'DROP MATERIALIZED VIEW IF EXISTS ' ||
        quote_ident(n.nspname) || '.' ||
        quote_ident(c.relname) ||
        ';' AS drop_ddl,

        'CREATE MATERIALIZED VIEW ' ||
        quote_ident(n.nspname) || '.' ||
        quote_ident(c.relname) ||

        CASE
            WHEN ts.spcname IS NOT NULL
            THEN E'\nTABLESPACE ' || quote_ident(ts.spcname)
            ELSE ''
        END ||

        E'\nAS\n' ||
        regexp_replace(pg_get_viewdef(c.oid, false), ';$', '') ||

        CASE
            WHEN c.relispopulated
            THEN E'\nWITH DATA;'
            ELSE E'\nWITH NO DATA;'
        END AS create_ddl

    FROM pg_class c

    JOIN pg_namespace n
        ON n.oid = c.relnamespace

    LEFT JOIN pg_tablespace ts
        ON ts.oid = c.reltablespace

    WHERE c.relkind = 'm'
      AND n.nspname = @schemaName
      AND c.relname = @name
),

materialized_view_indexes AS
(
    SELECT
        i.indrelid AS matview_oid,

        string_agg(
            pg_get_indexdef(i.indexrelid) || ';',
            E'\n'
            ORDER BY index_class.relname
        ) AS index_ddl

    FROM pg_index i

    JOIN pg_class index_class
        ON index_class.oid = i.indexrelid

    JOIN materialized_views mv
        ON mv.matview_oid = i.indrelid

    GROUP BY i.indrelid
),

export_scripts AS
(
    SELECT
        1 AS sort_order,
        mv.schema_name,
        mv.matview_name,
        'DROP' AS script_type,
        mv.drop_ddl AS ddl

    FROM materialized_views mv

    UNION ALL

    SELECT
        2 AS sort_order,
        mv.schema_name,
        mv.matview_name,
        'CREATE' AS script_type,

        mv.create_ddl ||
        CASE
            WHEN idx.index_ddl IS NOT NULL
            THEN E'\n\n' || idx.index_ddl
            ELSE ''
        END AS ddl

    FROM materialized_views mv

    LEFT JOIN materialized_view_indexes idx
        ON idx.matview_oid = mv.matview_oid
)

SELECT
    schema_name,
    matview_name,
    script_type,
    ddl

FROM export_scripts

ORDER BY
    matview_name,
    sort_order;
";*/


                //                string sql = @"
                //WITH materialized_views AS
                //(
                //    SELECT
                //        c.oid AS matview_oid,
                //        n.nspname AS schema_name,
                //        c.relname AS matview_name,

                //        'DROP MATERIALIZED VIEW IF EXISTS ' ||
                //        quote_ident(n.nspname) || '.' ||
                //        quote_ident(c.relname) ||
                //        ';' AS drop_ddl,

                //        'CREATE MATERIALIZED VIEW ' ||
                //        quote_ident(n.nspname) || '.' ||
                //        quote_ident(c.relname) ||

                //        CASE
                //            WHEN ts.spcname IS NOT NULL
                //            THEN E'\nTABLESPACE ' || quote_ident(ts.spcname)
                //            ELSE ''
                //        END ||

                //        E'\nAS\n' ||
                //        pg_get_viewdef(c.oid, false) ||

                //        CASE
                //            WHEN c.relispopulated
                //            THEN E'\nWITH DATA;'
                //            ELSE E'\nWITH NO DATA;'
                //        END AS create_ddl

                //    FROM pg_class c

                //    JOIN pg_namespace n
                //        ON n.oid = c.relnamespace

                //    LEFT JOIN pg_tablespace ts
                //        ON ts.oid = c.reltablespace

                //    WHERE c.relkind = 'm'
                //      AND n.nspname = @schemaName
                //),

                //materialized_view_indexes AS
                //(
                //    SELECT
                //        i.indrelid AS matview_oid,

                //        string_agg(
                //            pg_get_indexdef(i.indexrelid) || ';',
                //            E'\n'
                //            ORDER BY index_class.relname
                //        ) AS index_ddl

                //    FROM pg_index i

                //    JOIN pg_class index_class
                //        ON index_class.oid = i.indexrelid

                //    JOIN materialized_views mv
                //        ON mv.matview_oid = i.indrelid

                //    GROUP BY i.indrelid
                //),

                //export_scripts AS
                //(
                //    SELECT
                //        1 AS sort_order,
                //        mv.schema_name,
                //        mv.matview_name,
                //        'DROP' AS script_type,
                //        mv.drop_ddl AS ddl

                //    FROM materialized_views mv

                //    UNION ALL

                //    SELECT
                //        2 AS sort_order,
                //        mv.schema_name,
                //        mv.matview_name,
                //        'CREATE' AS script_type,

                //        mv.create_ddl ||
                //        CASE
                //            WHEN idx.index_ddl IS NOT NULL
                //            THEN E'\n\n' || idx.index_ddl
                //            ELSE ''
                //        END AS ddl

                //    FROM materialized_views mv

                //    LEFT JOIN materialized_view_indexes idx
                //        ON idx.matview_oid = mv.matview_oid
                //)

                //SELECT
                //    schema_name,
                //    matview_name,
                //    script_type,
                //    ddl

                //FROM export_scripts

                //ORDER BY
                //    matview_name,
                //    sort_order;
                //";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportMtViewAsync Failed");

                throw;
            }
        }

        public async Task<(bool success, string data)> ExportIndexAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportIndexAsync Started : {name}", name);

                string sql = @"
                    SELECT indexdef || ';' AS ddl
                    FROM pg_indexes
                    WHERE schemaname = @schemaName
                    AND indexname = @name;
                ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportIndexAsync Failed");
                //return (false, ex.Message);
                throw;
            }
        }

        //public async Task<(bool success, string data)> ExportTriggerAsync
        //(
        //    string schemaName,
        //    string name
        //)
        //{
        //    try
        //    {
        //        _logger.LogInformation("ExportTriggerAsync Started : {name}", name);

        //        string sql = @"
        //            SELECT pg_get_triggerdef(t.oid, true) || ';' AS ddl
        //            FROM pg_trigger t
        //            JOIN pg_class c ON c.oid = t.tgrelid
        //            JOIN pg_namespace n ON n.oid = c.relnamespace
        //            WHERE n.nspname = @schemaName
        //            AND t.tgname = @name
        //            AND NOT t.tgisinternal;
        //        ";

        //        return await ExecuteDdlQuery(sql, schemaName, name);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "ExportTriggerAsync Failed");
        //        throw;
        //    }
        //}
        public async Task<(bool success, string data)> ExportTriggerAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportTriggerAsync Started : {name}", name);

                string functionSql = @"
                    SELECT p.proname
                    FROM pg_trigger t
                    JOIN pg_proc p ON p.oid = t.tgfoid
                    JOIN pg_class c ON c.oid = t.tgrelid
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE n.nspname = @schemaName
                    AND t.tgname = @name
                    AND NOT t.tgisinternal;
                ";

                string[] paramNames =
                {
                    "@schemaName",
                    "@name"
                };

                DbType[] paramTypes =
                {
                    DbType.String,
                    DbType.String
                };

                object[] paramValues =
                {
                    schemaName,
                    name
                };

                DataTable functionResult = await _axDBService.ExecuteSelectQuery
                (
                    functionSql,
                    paramNames,
                    paramTypes,
                    paramValues
                );

                StringBuilder ddl = new StringBuilder();

                if (functionResult.Rows.Count > 0)
                {
                    string functionName = functionResult.Rows[0]["proname"].ToString();

                    var functionDdl = await ExportFunctionAsync(schemaName, functionName);

                    if (functionDdl.success)
                    {
                        ddl.AppendLine(functionDdl.data);
                    }
                }

                string triggerSql = @"
                                SELECT pg_get_triggerdef(t.oid, true) || ';' AS ddl
                                FROM pg_trigger t
                                JOIN pg_class c ON c.oid = t.tgrelid
                                JOIN pg_namespace n ON n.oid = c.relnamespace
                                WHERE n.nspname = @schemaName
                                AND t.tgname = @name
                                AND NOT t.tgisinternal;
                            ";

                var triggerDdl = await ExecuteDdlQuery
                (
                    triggerSql,
                    schemaName,
                    name
                );

                if (triggerDdl.success)
                {
                    ddl.AppendLine(triggerDdl.data);
                }

                _logger.LogInformation("ExportTriggerAsync Completed");

                return (true, ddl.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportTriggerAsync Failed");
                throw;
            }
        }

        public async Task<(bool success, string data)> ExportConstraintAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportConstraintAsync Started : {name}", name);

                string sql = @"
                    SELECT
                        'ALTER TABLE '
                        || quote_ident(n.nspname) || '.' || quote_ident(c.relname)
                        || ' ADD CONSTRAINT ' || quote_ident(con.conname)
                        || ' ' || pg_get_constraintdef(con.oid, true) || ';' AS ddl
                    FROM pg_constraint con
                    JOIN pg_class c ON c.oid = con.conrelid
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE n.nspname = @schemaName
                    AND con.conname = @name;
                ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportConstraintAsync Failed");
                throw;
            }
        }

        public async Task<(bool success, string data)> ExportSequenceAsync
        (
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExportSequenceAsync Started : {name}", name);

                //string sql = @"
                //    SELECT
                //        'CREATE SEQUENCE IF NOT EXISTS '
                //        || quote_ident(sequence_schema) || '.' || quote_ident(sequence_name)
                //        || ' START WITH ' || start_value
                //        || ' INCREMENT BY ' || increment
                //        || ' MINVALUE ' || minimum_value
                //        || ' MAXVALUE ' || maximum_value
                //        || ' CACHE ' || cache_size || ';' AS ddl
                //    FROM information_schema.sequences
                //    WHERE sequence_schema = @schemaName
                //    AND sequence_name = @name;
                //";
//                string sql = @"
//    SELECT
//        'CREATE SEQUENCE IF NOT EXISTS '
//        || quote_ident(sequence_schema)
//        || '.'
//        || quote_ident(sequence_name)
//        || ' START WITH ' || start_value
//        || ' INCREMENT BY ' || increment
//        || ' MINVALUE ' || minimum_value
//        || ' MAXVALUE ' || maximum_value
//        || ';' AS ddl
//    FROM information_schema.sequences
//    WHERE sequence_schema = @schemaName
//    AND sequence_name = @name;
//";
                string sql = @"
                       SELECT
                            'CREATE SEQUENCE IF NOT EXISTS '
                            || quote_ident(schemaname)
                            || '.'
                            || quote_ident(sequencename)
                            || ' START WITH ' || start_value
                            || ' INCREMENT BY ' || increment_by
                            || ' MINVALUE ' || min_value
                            || ' MAXVALUE ' || max_value
                            || ' CACHE ' || cache_size
                            || ';' AS ddl
                        FROM pg_sequences
                        WHERE schemaname = @schemaName
                        AND sequencename = @name;
                    ";

                return await ExecuteDdlQuery(sql, schemaName, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportSequenceAsync Failed");
                throw;
            }
        }

        public async Task<(bool success,string data)> ExportTableAsync
        (
            string schemaName,
            string tableName
        )
        {
            try
            {
                _logger.LogInformation("ExportTableAsync Started : {tableName}", tableName);

                bool success = false;
                string data = null;
                string sql = @"
                    SELECT
                        column_name,
                        data_type,
                        udt_name,
                        character_maximum_length,
                        numeric_precision,
                        numeric_scale,
                        is_nullable,
                        column_default
                    FROM information_schema.columns
                    WHERE table_schema = @schemaName
                    AND table_name = @tableName
                    ORDER BY ordinal_position;
                ";

                string[] paramNames =
                {
                    "@schemaName",
                    "@tableName"
                };

                DbType[] paramTypes =
                {
                    DbType.String,
                    DbType.String
                };

                object[] paramValues =
                {
                    schemaName,
                    tableName
                };

                DataTable result = await _axDBService.ExecuteSelectQuery
                (
                    sql,
                    paramNames,
                    paramTypes,
                    paramValues
                );

                if (result.Rows.Count > 0)
                {
                    List<string> columns = new List<string>();

                    foreach (DataRow row in result.Rows)
                    {
                        string columnName = row["column_name"].ToString();

                        string dataType = row["data_type"].ToString();

                        string udtName = row["udt_name"].ToString();

                        object maxLength = row["character_maximum_length"];

                        object precision = row["numeric_precision"];

                        object scale = row["numeric_scale"];

                        string isNullable = row["is_nullable"].ToString();

                        object defaultValue = row["column_default"];

                        string finalType = GetPostgresDataType
                        (
                            dataType,
                            udtName,
                            maxLength,
                            precision,
                            scale
                        );

                        string col = $"    \"{columnName}\" {finalType}";

                        if (defaultValue != DBNull.Value)
                        {
                            col += $" DEFAULT {defaultValue}";
                        }

                        if (isNullable == "NO")
                        {
                            col += " NOT NULL";
                        }

                        columns.Add(col);
                    }

                    //var dbData = _axDBService.GetDbDetails();

                    //string ddl = $"""
                    //CREATE TABLE IF NOT EXISTS "{schemaName}"."{tableName}" (
                    //{string.Join("," + Environment.NewLine, columns)}
                    //);
                    //""";

                    //string ddl =
                    //      "CREATE TABLE IF NOT EXISTS \"{{schemaName}}\".\"" + tableName + "\" (" +
                    //      Environment.NewLine +
                    //      string.Join("," + Environment.NewLine, columns) +
                    //      Environment.NewLine +
                    //      ");";
                    string ddl =
                                "CREATE TABLE IF NOT EXISTS \"{{schemaName}}\".\"" + tableName + "\" (" +
                                Environment.NewLine +
                                string.Join("," + Environment.NewLine, columns) +
                                Environment.NewLine +
                                ");" +
                                Environment.NewLine +
                                "$D#";

                    _logger.LogDebug(ddl);

                    _logger.LogInformation("ExportTableAsync Completed");

                    return (success = true,data = ddl);
                }
                else
                {
                    _logger.LogInformation("No Records Found");
                    //return (success = false,data = $"Table not found : {schemaName}.{tableName}");
                    throw new Exception($"Table not found : {schemaName}.{tableName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportTableAsync Failed");

                throw;
            }
        }

        private string GetPostgresDataType
        (
            string dataType,
            string udtName,
            object maxLength,
            object precision,
            object scale
        )
        {
            return dataType switch
            {
                "character varying" => maxLength != DBNull.Value
                    ? $"varchar({maxLength})"
                    : "varchar",

                "character" => maxLength != DBNull.Value
                    ? $"char({maxLength})"
                    : "char",

                "numeric" => precision != DBNull.Value && scale != DBNull.Value
                    ? $"numeric({precision},{scale})"
                    : "numeric",

                "ARRAY" => udtName.TrimStart('_') + "[]",

                "USER-DEFINED" => udtName,

                _ => dataType
            };
        }

        private async Task<(bool success,string data)> ExecuteDdlQuery
        (
            string sql,
            string schemaName,
            string name
        )
        {
            try
            {
                _logger.LogInformation("ExecuteDdlQuery Started");

                bool success = false ;
                string data = null;
                string[] paramNames =
                {
                    "@schemaName",
                    "@name"
                };

                DbType[] paramTypes =
                {
                    DbType.String,
                    DbType.String
                };

                object[] paramValues =
                {
                    schemaName,
                    name
                };

                DataTable result = await _axDBService.ExecuteSelectQuery
                (
                    sql,
                    paramNames,
                    paramTypes,
                    paramValues
                );

                if (result.Rows.Count > 0)
                {
                    StringBuilder ddl = new StringBuilder();

                    dynamic dbdetails =await _axDBService.GetDbDetails();


                    //string connectionString = dbdetails.ConnectionString;

                    //string schemaNameFromDb = connectionString
                    //.Split(';')
                    //.FirstOrDefault(x => x.StartsWith("Uid=", StringComparison.OrdinalIgnoreCase))
                    //?.Split('=')[1];

                    var connectionField = dbdetails.GetType().GetField(
                                            "_connection",
                                            BindingFlags.NonPublic | BindingFlags.Instance);

                    var connection = connectionField?.GetValue(dbdetails);

                    var userNameProperty = connection?.GetType().GetProperty("UserName");

                    string schemaNameFromDb = userNameProperty?.GetValue(connection)?.ToString();

                    //var schemaNameFromDb = dbdetails._connection.UserName.ToString();

                    foreach (DataRow row in result.Rows)
                    {
                        //ddl.AppendLine(row["ddl"].ToString());

                        //ddl.Append("$D#");
                        string ddlQuery = row["ddl"].ToString();

                        ddlQuery = ddlQuery.Replace(schemaNameFromDb, "\"{{schemaName}}\"");

                        _logger.LogDebug(ddlQuery);

                        ddl.AppendLine(ddlQuery);
                        ddl.Append("$D#");
                    }

                    _logger.LogInformation("ExecuteDdlQuery Completed");


                    return (success = true, data = ddl.ToString());
                }
                else
                {
                    _logger.LogInformation("No Records Found");

                    //return (success = false, data = $"No records found for : {name}");

                    throw new Exception($"No records found for : {name}");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteDdlQuery Failed");

                throw;

            }
        }
    }
}