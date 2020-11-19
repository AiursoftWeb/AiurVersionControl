using AiurEventSyncer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.CompilerServices;

namespace AiurEventSyncer.WebExtends
{
    public static class WebExtends
    {
        public static IActionResult Repository<T>(this ControllerBase controller, Repository<T> repository)
        {
            return null;
        }
    }
}
