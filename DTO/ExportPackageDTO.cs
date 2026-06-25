//namespace AxiDataPackages.DTOs
//{
//    public class ExportPackageDTO
//    {
//        public string TransId { get; set; }

//        public string ProjectName { get; set; }

//        public string UserName { get; set; }

//        public string Password { get; set; }
//    }
//}
namespace AxiDataPackages.DTO
{
    public class ExportPackageDTO
    {
        public string RequestId { get; set; }

        public string AppName { get; set; }

        public string PackageName { get; set; }

        public string PackageVersion { get; set; }

        public string RequestedBy { get; set; }

        public string Password { get; set; }

        public string ExportDir { get; set; }

        public bool trace { get; set; }

        public ObjectDTO Objects { get; set; }
    }

    public class ObjectDTO
    {
        public string[]? tstruct { get; set; }

        public string[]? iview { get; set; }

        public string[]? custompage { get; set; }

        public string[]? userrole { get; set; }

        public string[]? usergroup { get; set; }

        public string[]? developeroption { get; set; }

        public string[]? uiplugin { get; set; }

        public string[]? axvars { get; set; }

        public string[]? tables{ get; set; }

        public string[]? views { get; set; }

        public string[]? functions { get; set; }

        public string[]? constraint { get; set; }

        public string[]? index { get; set; }

        public string[]? triggers { get; set; }

        public string[]? sequence { get; set; }

        public string[]? procedures { get; set; }


    }
}