(function ($, window) {
    'use strict';

    window.waadSelector = function (namespace, realm, selectorElement, returnUrl) {
        if (!returnUrl) {
            if (document.location.hash) {
                returnUrl = document.location.hash;
            }
        }

        var script = document.createElement('script');
        script.src = 'https://' + namespace + '.accesscontrol.windows.net/v2/metadata/IdentityProviders.js?protocol=wsfederation&realm=' + realm + '&context=' + escape(returnUrl) + '&request_id=&version=1.0&callback=waadIdentityProvidersLoaded';
        document.getElementsByTagName('head')[0].appendChild(script);
        window.waadSelectorElement = selectorElement;
    };

    window.waadIdentityProvidersLoaded = function (identityProviders) {
        var selector = window.waadSelectorElement;
        $(selector).html('');
        if (identityProviders.length == 0) {
            $(selector).append('<p>No identity providers were configured</p>');
            return;
        }

        var list = '';
        $.each(identityProviders, function (index, idp) {
            var link = '<a href="' + idp.LoginUrl + '" alt="Login with ' + idp.Name + '" class="selector-' + slugify(idp.Name) + '">' + idp.Name + '</a>';
            list += "<li>" + link + "</li>";
        });

        function slugify(text) {
            text = text.replace(/[^-a-zA-Z0-9,&\s]+/ig, '');
            text = text.replace(/-/gi, "_");
            text = text.replace(/\s/gi, "-");
            return text;
        }

        $(selector).append($('<ul>').append(list));
    };
})(jQuery, window);