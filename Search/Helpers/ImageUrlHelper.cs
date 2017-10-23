using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Search.Helpers
{
    public static class ImageUrlHelper
    {
        public static IHtmlString Generate(string imageUrl, int? maxWidth = null)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return new MvcHtmlString(string.Empty);
            }

            var url = new StringBuilder();
            var querystringAppend = (imageUrl.IndexOf('?') > 0 ? '&' : '?');

            url.Append(imageUrl);
            if (maxWidth > 0)
            {

                url.Append($"{querystringAppend}mw={maxWidth}");
                //If maxWidth is specified, a hash will be appended to prevent issues with caching the wrong size image.
                return new MvcHtmlString(url.ToString());
            }

            return new MvcHtmlString(url.ToString());
        }

        public static IHtmlString Generate(string imageUrl, int width, int height, bool crop = false)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return new MvcHtmlString(string.Empty);
            }

            var url = new StringBuilder();
            var querystringAppend = (imageUrl.IndexOf('?') > 0 ? '&' : '?');

            url.Append(imageUrl);

            url.Append(crop ? $"{querystringAppend}w={width}&h={height}&CenterCrop=1&useCustomFunctions=1" : $"{querystringAppend}mw={width}&mh={height}");

            //Append projection hash and return
            return new MvcHtmlString(url.ToString());
        }
    }
}