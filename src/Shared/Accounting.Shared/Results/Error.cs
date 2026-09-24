namespace Accounting.Shared.Results;

public sealed record Error(
    string Code ,
    string Message ,
    string Status ,
    string Field);