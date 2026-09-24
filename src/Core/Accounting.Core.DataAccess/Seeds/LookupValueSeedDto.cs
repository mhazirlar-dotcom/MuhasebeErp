namespace Accounting.Core.DataAccess.Seeds;

internal sealed class LookupValueSeedDto
{
    #region Properties
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ParentCode { get; set; } = string.Empty;
    #endregion Properties
}