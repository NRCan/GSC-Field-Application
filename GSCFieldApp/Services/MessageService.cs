using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace GSCFieldApp.Services
{
    public sealed class MessageService
    {
        private readonly IStringLocalizer<MessageService> _localizer = null!;

        public MessageService(IStringLocalizer<MessageService> localizer) =>
            _localizer = localizer;

        [return: NotNullIfNotNull(nameof(_localizer))]
        public string? GetLocalString(string key)
        {
            LocalizedString localizedString = _localizer[key];
            return localizedString;
        }

        public MessageService()
        { }
    }
}
