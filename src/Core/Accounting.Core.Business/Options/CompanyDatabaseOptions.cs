namespace Accounting.Core.Business.Options;

public class CompanyDatabaseOptions
{
    #region Constants
    public const string SectionName = "CompanyDatabase";
    #endregion Constants

    #region Properties
    public string ServerName { get; set; } = ".";
    public bool IntegratedSecurity { get; set; } = true;
    public string DbUserName { get; set; } = string.Empty;
    public string DbPassword { get; set; } = string.Empty;
    public string DatabaseNameTemplate { get; set; } = "Accounting_{Id}";
    #endregion Properties

    #region Operations
    public string BuildDatabaseName(Guid companyId)
    {
        return DatabaseNameTemplate.Replace("{Id}" , companyId.ToString("N"));
    }
    #endregion Operations
}