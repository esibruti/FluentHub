﻿using FluentHub.Octokit.Models;
using Octokit.GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Octokit.Queries.Organizations
{
    public class OrganizationQueries
    {
        public OrganizationQueries() => new App();

        public async Task<Models.Organization> GetOverview(string org)
        {
            var query = new Query()
                    .Organization(org)
                    .Select(x => new
                    {
                        AvatarUrl = x.AvatarUrl(100),
                        x.Description,
                        x.Email,
                        x.IsVerified,
                        x.Location,
                        x.Login,
                        x.Name,
                        x.WebsiteUrl,
                    })
                    .Compile();

            var result = await App.Connection.Run(query);

            // Convert
            Models.Organization item = new();
            item.AvatarUrl = result.AvatarUrl;
            item.Description = result.Description;
            item.Email = result.Email;
            item.IsVerified = result.IsVerified;
            item.Location = result.Location;
            item.Login = result.Login;
            item.Name = result.Name;
            item.WebsiteUrl = result.WebsiteUrl;

            return item;
        }
    }
}
