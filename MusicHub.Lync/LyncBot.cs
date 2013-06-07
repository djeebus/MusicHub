using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;

namespace MusicHub.Lync
{
    public class LyncBot
    {
        private readonly LyncClient _client;
        private readonly ConversationService _conversationService;

        public const string UserUri = "jlombrozo@musichub";
        public const string Username = @"kabbage\jlombrozo";
        public const string Password = "Kabbage123!";

        public LyncBot()
        {
            this._client = LyncClient.GetClient();
            if (this._client == null)
                throw new ArgumentNullException("_client", "Unable to retrieve client");

            this._client.CredentialRequested += _client_CredentialRequested;
            this._client.StateChanged += _client_StateChanged;

            this._conversationService = new ConversationService(this._client.ConversationManager);

            if (this._client.InSuppressedMode)
            {
                if (this._client.State == ClientState.Uninitialized)
                    this.Initialize();
            }
        }

        public void Initialize()
        {
            this._client.BeginInitialize(EndInitialize, null);
        }

        private void EndInitialize(IAsyncResult ar)
        {
            this._client.EndInitialize(ar);
        }

        void _client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            switch (this._client.State)
            {
                case ClientState.Initializing:
                    break;

                case ClientState.SignedOut:
                    this._client.BeginSignIn(UserUri,  Username, Password, EndSignIn, null);
                    break;
            }
        }

        private void EndSignIn(IAsyncResult ar)
        {
            this._client.EndSignIn(ar);
        }

        void _client_CredentialRequested(object sender, CredentialRequestedEventArgs e)
        {
            switch (e.Type)
            {
                case CredentialRequestedType.SignIn:
                    e.Submit(e.UserName, Password, false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("e.Type", e.Type, "Unknown credential type");
            }            
        }
    }
}
