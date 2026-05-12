namespace MicroLendingSystem.Backend.Authorization;

public static class PermissionNames
{
    public const string Loan_Read = "Loan Read";
    public const string Loan_Create = "Loan Create";
    public const string Loan_Approve = "Loan Approve";
    public const string Loan_Repay = "Loan Repay";
    public const string Loan_Update = "Loan Update";
    public const string Loan_Delete = "Loan Delete";
    public const string LoanRequest_List = "Loan Request List";

    public const string User_Read = "User Read";
    public const string User_Create = "User Create";
    public const string User_Update = "User Update";
    public const string User_Delete = "User Delete";
    public const string User_AssignRole = "User Assign Role";

    public const string Role_Read = "Role Read";
    public const string Role_Create = "Role Create";
    public const string Role_Update = "Role Update";
    public const string Role_Delete = "Role Delete";
    public const string Role_AssignPermissions = "Role Assign Permissions";

    public const string Permission_Read = "Permission Read";
    public const string Permission_Create = "Permission Create";
    public const string Permission_Update = "Permission Update";
    public const string Permission_Delete = "Permission Delete";

    public const string Borrower_Read = "Borrower Read";
    public const string Borrower_Create = "Borrower Create";
    public const string Borrower_Update = "Borrower Update";
    public const string Borrower_Delete = "Borrower Delete";

    public const string LoanSetting_Read = "Loan Setting Read";
    public const string LoanSetting_Create = "Loan Setting Create";
    public const string LoanSetting_Update = "Loan Setting Update";
    public const string LoanSetting_Delete = "Loan Setting Delete";

    public const string Transaction_List = "Transaction List";
    public const string Transaction_Export = "Transaction Export";

    public const string AdminRoleName = "Admin";
}
