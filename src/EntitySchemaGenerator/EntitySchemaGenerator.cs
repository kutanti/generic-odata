using EntitySchemaGenerator.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntitySchemaGenerator
{
    public class EntitySchemaGenerator : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IEntityGeneratorService _entityGenerator;

        public EntitySchemaGenerator(IHostApplicationLifetime appLifetime, IEntityGeneratorService entityGenerator)
        {
            _appLifetime = appLifetime;
            _entityGenerator = entityGenerator;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _entityGenerator.ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {

                        _appLifetime.StopApplication();
                    }
                });
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}