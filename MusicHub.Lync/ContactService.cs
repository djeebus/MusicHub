using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;

namespace MusicHub.Lync
{
    public class ContactService
    {
        private readonly ContactManager _contactManager;

        public ContactService(ContactManager contactManager)
        {
            if (contactManager == null)
                throw new ArgumentNullException("contactManager");

            this._contactManager = contactManager;

            foreach (var group in this._contactManager.Groups)
            {
                var contactCollection = group as ContactCollection;
                if (contactCollection == null)
                    throw new ArgumentOutOfRangeException();

                foreach (var contact in contactCollection)
                {
                    contact.ContactInformationChanged += contact_ContactInformationChanged;
                }
            }
        }

        void contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            var contact = sender as Contact;

            foreach (var info in e.ChangedContactInformation)
            {
                switch (info)
                {
                    case ContactInformationType.Availability:
                        var availability = (ContactAvailability)contact.GetContactInformation(info);
                        switch (availability)
                        {
                            case ContactAvailability.Free:
                                // set user online
                                break;

                            default:
                                // set user offline
                                break;
                        }
                        break;

                    
                }
            }
        }
    }
}
