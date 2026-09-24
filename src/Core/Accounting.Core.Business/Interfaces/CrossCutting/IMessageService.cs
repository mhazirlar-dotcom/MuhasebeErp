using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.CrossCutting;

public interface IMessageService
{
    #region Operations
    void ShowSuccess(string message);
    void ShowSuccess(string title , string message);

    void ShowInfo(string message);
    void ShowInfo(string title , string message);

    void ShowWarning(string message);
    void ShowWarning(string title , string message);

    void ShowError(string message);
    void ShowError(string title , string message);

    void ShowErrors(string title , IReadOnlyList<Error> errors);

    bool Confirm(string title , string message);
    #endregion Operations
}