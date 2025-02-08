namespace CPCoded.Models.ViewModels
{
    public class UserDetailsViewModel
    {
       
            public ApplicationUser User { get; set; }
            public List<LoanApplication> Loans { get; set; }
        

    }
}
