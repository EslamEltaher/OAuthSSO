using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;
        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MustOwnImageRequirement requirement)
        {
            #region FROM MVC the context.Resource is cast to AuthorizationFilterContext
            var filterContext = context.Resource as AuthorizationFilterContext;
            if(filterContext == null)
            {
                context.Fail();
                return;
            }
            #endregion

            var imageId = filterContext.RouteData.Values["id"].ToString();

            Guid imageGuid;
            if(!Guid.TryParse(imageId, out imageGuid))
            {
                context.Fail();
                return;
            }

            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if(!_galleryRepository.IsImageOwner(imageGuid, ownerId))
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
