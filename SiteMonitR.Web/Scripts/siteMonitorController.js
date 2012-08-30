/// <reference path="jquery-1.8.0.js" />
/// <reference path="jquery.signalR-0.5.3.js" />
/// <reference path="knockout-2.1.0.js" />
// ---------------------------------------------------------------------------------- 
// Microsoft Developer & Platform Evangelism 
//  
// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// ---------------------------------------------------------------------------------- 
// The example companies, organizations, products, domain names, 
// e-mail addresses, logos, people, places, and events depicted 
// herein are fictitious.  No association with any real company, 
// organization, product, domain name, email address, logo, person, 
// places, or events is intended or should be inferred. 
// ----------------------------------------------------------------------------------
$(function () {

    $('#noSitesToMonitorMessage').hide();

    function SiteStatusItem(u, test, s, t) {
        var self = this;
        self.url = u;
        self.test = test;
        self.cssClass = ko.observable(s);
        self.siteStatus = ko.observable(t);
    }

    function GridViewModel(sites) {
        var self = this;
        self.items = ko.observableArray(sites);
    }

    function controller() {
        var self = this;
        self.model = new GridViewModel([]);
        self.connection = $.hubConnection();
        self.connection.logging = true;
        self.siteMonitorHub = self.connection.createProxy("SiteMonitR");

        this.updateSite = function (url, testResult, cssClass, siteStatus) {
            self.model.items().forEach(function (site) {
                if (site.url === url) {
                    site.cssClass(cssClass);
                    site.siteStatus(siteStatus);
                    site.testResult = testResult;
                }
            });
        };

        self.addSite = function (url, test) {
            if ($('.site[data-url="' + url + '"]').length == 0) {
                var site = new SiteStatusItem(url, test, 'btn-warning', 'Waiting');
                self.model.items.push(site);
            }
        };

        self.updateSiteStatus = function (monitorUpdate) {
            if (monitorUpdate.Result.ping == true) {
                self.updateSite(monitorUpdate.Url, monitorUpdate.Result.testResult, 'btn-success', 'Up');
            }
            else {
                self.updateSite(monitorUpdate.Url, monitorUpdate.Result.testResult, 'btn-danger', 'Down');
            }
        };

        self.toggleSpinner = function (isVisible) {
            if (isVisible == true)
                $('#spin').show();
            if (isVisible != true)
                $('#spin').hide();
        };

        self.toggleGrid = function () {
            if ($('.site').length == 0) {
                $('#noSitesToMonitorMessage').show();
                $('#sites').hide();
            }
            else {
                $('#noSitesToMonitorMessage').hide();
                $('#sites').show();
            }
        }
    }

    var c = new controller();

    c.siteMonitorHub
        .on('serviceIsUp', function () {
            c.toggleSpinner(true);
            c.siteMonitorHub.invoke('getSiteList');
        })
        .on('siteListObtained', function (sites) {
            $(sites).each(function (i, site) {
                c.addSite(site.Url, site.Test);
            });
            c.toggleSpinner(false);
            c.toggleGrid();
        })
        .on('siteStatusUpdated', function (monitorUpdate) {
            c.updateSiteStatus(monitorUpdate);
            c.toggleSpinner(false);
        })
        .on('siteAddedToGui', function (url) {
            $('#siteUrl').val('http://');
            $('#siteUrl').focus();
            c.toggleSpinner(false);
            c.toggleGrid();
        })
        .on('siteRemovedFromGui', function (url) {
            $('.site[data-url="' + url + '"]').remove();
            c.toggleGrid();
            c.toggleSpinner(false);
        })
        .on('checkingSite', function (url) {
            c.toggleSpinner(false);
            c.updateSite(url, 'btn-info', 'Checking');
        });

    $('#addSite').click(function () {
        var u = $('#siteUrl').val();
        var test = $('#test').val();
        c.addSite(u, test);
        c.toggleSpinner(true);
        c.siteMonitorHub.invoke('addSite', u, test);
    });

    $('.removeSite').live('click', function () {
        c.toggleSpinner(true);
        var url = $(this).data('url');

        $('.site[data-url="' + url + '"]').fadeOut('fast', function () {
            $('.site[data-url="' + url + '"]').remove();
        });

        c.siteMonitorHub.invoke('removeSite', url);
    });

    $('[data-identifier="showResult"]').live('click', function () {
        var url = $(this).data('url');
        c.model.items().forEach(function (site) {
            if (site.url == url) {
                alert(site.testResult);
            }
        });
    });

    c.connection.start().done(function () {
        c.toggleSpinner(true);
        c.siteMonitorHub.invoke('getSiteList');
    });

    ko.applyBindings(c.model);

    $('#siteUrl').val('http://');
});