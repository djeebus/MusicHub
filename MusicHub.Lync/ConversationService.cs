using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace MusicHub.Lync
{
    public class ConversationService
    {
        private readonly ConversationManager _conversationManager;

        public ConversationService(ConversationManager conversationManager)
        {
            if (conversationManager == null)
                throw new ArgumentNullException("conversationManager");

            this._conversationManager = conversationManager;
            this._conversationManager.ConversationAdded += _conversationManager_ConversationAdded;
        }

        void _conversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            var modalities = e.Conversation.Modalities;

            foreach (var modality in modalities)
            {
                switch (modality.Key)
                {
                    case ModalityTypes.AudioVideo:
                        switch (modality.Value.State)
                        {
                            case ModalityState.Notified:
                                modality.Value.Reject(ModalityDisconnectReason.NotAcceptableHere);
                                break;
                        }
                        break;

                    case ModalityTypes.InstantMessage:
                        switch (modality.Value.State)
                        {
                            case ModalityState.Connected:
                                var imModality = modalities[ModalityTypes.InstantMessage] as InstantMessageModality;
                                if (imModality == null)
                                    throw new ArgumentOutOfRangeException("modality", modalities[ModalityTypes.InstantMessage], "InstantMessageModality expected");
                                
                                StartConversation(e.Conversation, imModality);
                                break;
                        }
                        break;
                }
            }
        }

        private void StartConversation(Conversation conversation, InstantMessageModality imModality)
        {
            conversation.ParticipantAdded += conversation_ParticipantAdded;
            conversation.StateChanged += conversation_StateChanged;
            conversation.ActionAvailabilityChanged += conversation_ActionAvailabilityChanged;

            imModality.InstantMessageReceived += imModality_InstantMessageReceived;
            imModality.IsTypingChanged += imModality_IsTypingChanged;
            imModality.ActionAvailabilityChanged += imModality_ActionAvailabilityChanged;
        }

        private void conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void conversation_ActionAvailabilityChanged(object sender, ConversationActionAvailabilityEventArgs e)
        {
            throw new NotImplementedException();
        }

        void conversation_StateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void imModality_ActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void imModality_IsTypingChanged(object sender, IsTypingChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void imModality_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
