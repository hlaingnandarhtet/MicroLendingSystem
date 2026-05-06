using microlending_API.Features.Loans.Constants;

namespace microlending_API.Features.Loans;

public class LoanDto
{
    // ALL Loan List
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

// Get By Id
public class LoanDetails : LoanDto
{
    public int BorrowerId { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

// Create Loan 
public class CreateLoanRequest
{
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
}

//Filter & Pagination
public class GetLoansRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public LoanStatus? Status { get; set; }
}

// Result Pattern Wrapper
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

// Update Loans
public class UpdateLoanDataRequest
{
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
}