using System;

namespace LegacyApp
{

    public interface IUserCreditService : IDisposable
    {
        int GetCreditLimit(string lastName, DateTime dateOfBirth);
    }

    public interface IClientRepository
    {
        Client GetById(int clientId);
    }
    
    public class UserService
    {

        private IUserCreditService _userCreditService;
        private IClientRepository _clientRepository;
        
        public UserService(IUserCreditService creditService, IClientRepository clientRepository)
        {
            _userCreditService = creditService;
            _clientRepository = clientRepository;
        }
        
        public UserService()
        { 
            _userCreditService = new UserCreditService();
            _clientRepository = new ClientRepository();
        }

        private static bool _validateName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }
            
            return true;
        }

        private static bool _validateEmail(string email)
        {
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            return true;
        }

        private static bool _validateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            return true;
        }

        private void _setCreditLimit(string typeClient, User user, int creditLimit)
        {
            if (typeClient == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (typeClient == "ImportantClient")
            {
                using (_userCreditService)
                {
                    user.CreditLimit = creditLimit * 2;
                }
            }
            else
            {
                user.HasCreditLimit = true;
                using (_userCreditService)
                {
                    user.CreditLimit = creditLimit;
                }
            }
        }

        private int _getCreditLimit(User user, IUserCreditService creditService)
        {
            using (creditService)
            {
               return _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth); 
            }
        }

        private static bool _validateClientCreditLimit(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            return true;
        }
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            
            if (!_validateName(firstName, lastName) || 
                !_validateEmail(email) || 
                !_validateAge(dateOfBirth))
            {
                return false;
            }
            
            
            var client = _clientRepository.GetById(clientId);
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            
            _setCreditLimit(client.Type, user, _getCreditLimit(user, _userCreditService));

            if (!_validateClientCreditLimit(user))
                return false;
            
            
            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
