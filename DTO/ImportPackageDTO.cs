namespace AxiDataPackages.DTO
{
    public class ImportPackageDTO
    {
        public string RequestId { get; set; }

        public string AppName { get; set; }

        public string PackageName { get; set; }

        public string PackageVersion { get; set; }

        public string RequestedBy { get; set; }

        public string Password { get; set; }

        public string ImportDir { get; set; }

        public bool? trace { get; set; }
    }
}
