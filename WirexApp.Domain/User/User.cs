using System;
using System.Collections.Generic;
using WirexApp.Domain.UserAccounts;

namespace WirexApp.Domain.User
{
    public class User : AggregateRoot // IAggregateRoot where T : DomainEventBase
    {
        public UserId _userId;     

        private string _firstName;

        private string _lastName;

        private string _adresse;

        private string _eMail;

        private User()
        {
            
        }
        public User(string firstName, string lastName, string adresse, string eMail)
        {
            this._userId = new UserId(Guid.NewGuid());
            this._firstName = firstName;
            this._lastName = lastName;
            this._adresse = adresse;
            this._eMail = eMail;
            this.AddDomainEvent(new UserRegisteredEvent(this._userId));
        }

    }
}
