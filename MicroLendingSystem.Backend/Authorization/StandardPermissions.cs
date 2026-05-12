namespace MicroLendingSystem.Backend.Authorization;

/// <summary>Canonical permission display names (spaces) for seeding and RBAC UI.</summary>
public static class StandardPermissions
{
    public static IReadOnlyList<string> All { get; } =
    [
        PermissionNames.Loan_Read,
        PermissionNames.Loan_Create,
        PermissionNames.Loan_Approve,
        PermissionNames.Loan_Repay,
        PermissionNames.Loan_Update,
        PermissionNames.Loan_Delete,
        PermissionNames.LoanRequest_List,
        PermissionNames.User_Read,
        PermissionNames.User_Create,
        PermissionNames.User_Update,
        PermissionNames.User_Delete,
        PermissionNames.User_AssignRole,
        PermissionNames.Role_Read,
        PermissionNames.Role_Create,
        PermissionNames.Role_Update,
        PermissionNames.Role_Delete,
        PermissionNames.Role_AssignPermissions,
        PermissionNames.Permission_Read,
        PermissionNames.Permission_Create,
        PermissionNames.Permission_Update,
        PermissionNames.Permission_Delete,
        PermissionNames.Borrower_Read,
        PermissionNames.Borrower_Create,
        PermissionNames.Borrower_Update,
        PermissionNames.Borrower_Delete,
        PermissionNames.LoanSetting_Read,
        PermissionNames.LoanSetting_Create,
        PermissionNames.LoanSetting_Update,
        PermissionNames.LoanSetting_Delete,
        PermissionNames.Transaction_List,
        PermissionNames.Transaction_Export
    ];

    /// <summary>Maps legacy underscore names to current display names (startup migration).</summary>
    public static IReadOnlyList<(string OldName, string NewName)> LegacyRenames { get; } =
    [
        ("Loan_Read", "Loan Read"),
        ("Loan_Create", "Loan Create"),
        ("Loan_Approve", "Loan Approve"),
        ("Loan_Repay", "Loan Repay"),
        ("Loan_Update", "Loan Update"),
        ("Loan_Delete", "Loan Delete"),
        ("LoanRequest_List", "Loan Request List"),
        ("User_Read", "User Read"),
        ("User_Create", "User Create"),
        ("User_Update", "User Update"),
        ("User_Delete", "User Delete"),
        ("User_AssignRole", "User Assign Role"),
        ("Role_Read", "Role Read"),
        ("Role_Create", "Role Create"),
        ("Role_Update", "Role Update"),
        ("Role_Delete", "Role Delete"),
        ("Role_AssignPermissions", "Role Assign Permissions"),
        ("Permission_Read", "Permission Read"),
        ("Permission_Create", "Permission Create"),
        ("Permission_Update", "Permission Update"),
        ("Permission_Delete", "Permission Delete"),
        ("Borrower_Read", "Borrower Read"),
        ("Borrower_Create", "Borrower Create"),
        ("Borrower_Update", "Borrower Update"),
        ("Borrower_Delete", "Borrower Delete"),
        ("LoanSetting_Read", "Loan Setting Read"),
        ("LoanSetting_Create", "Loan Setting Create"),
        ("LoanSetting_Update", "Loan Setting Update"),
        ("LoanSetting_Delete", "Loan Setting Delete"),
        ("Transaction_List", "Transaction List")
    ];
}
