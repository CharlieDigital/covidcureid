global using System;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Text.Json;

global using CovidCureIdApp.DataAccess;
global using CovidCureIdApp.DataAccess.Support;
global using CovidCureIdApp.Model;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;

global using Microsoft.Azure.Cosmos;
global using Microsoft.Azure.Cosmos.Linq;
global using Microsoft.Azure.Functions.Extensions.DependencyInjection;
global using Microsoft.Azure.WebJobs;
global using Microsoft.Azure.WebJobs.Extensions.Http;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;