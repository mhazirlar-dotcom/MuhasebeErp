namespace Accounting.Desktop.ViewModels;

public sealed class FirmClassItem(int value , string name)
{
    #region Properties
    public int Value { get; } = value;
    public string Name { get; } = name;
    #endregion Properties
}