namespace WCFPractise
{
    public class CustomUsernamePasswordValidator : CoreWCF.IdentityModel.Selectors.UserNamePasswordValidator
    {
        public override ValueTask ValidateAsync(string userName, string password)
        {
            bool valid = userName.ToLowerInvariant().StartsWith("valid")
                && password.ToLowerInvariant().StartsWith("valid");
            if (!valid)
            {
                throw new FaultException("Unknown Username or Incorrect Password");
            }
            return new ValueTask();
        }

        public static void AddToHost(ServiceHostBase host)
        {
            var srvCredentials = new ServiceCredentials();
            srvCredentials.UserNameAuthentication.UserNamePasswordValidationMode = CoreWCF.Security.UserNamePasswordValidationMode.Custom;
            srvCredentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUsernamePasswordValidator();
            host.Description.Behaviors.Add(srvCredentials);
        }

    }
}
