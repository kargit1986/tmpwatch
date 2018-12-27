using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;
using Google.Cloud.Storage.V1;
using Google.Cloud.PubSub.V1;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace gcs_watch_net
{
    class SecretMetadata {
        public string Name { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
    }


    public class Startup
    {
        const string ProjectId = "api-7805202409062569548-609752";
        const string BucketName = "testbuck1111111";
        const string SubsciptionId =  "vaultsb";
        const string topicId = "vaultchange";
        public async Task GetFromStorage()
        {
            var client = await StorageClient.CreateAsync();
            MemoryStream ms = new MemoryStream();
            await client.DownloadObjectAsync(BucketName,"data.json",ms);
            ms.Position = 0;
            var str = Encoding.Default.GetString(ms.ToArray());
            Console.WriteLine("stirng is ");
            Console.WriteLine(str);
        }

        public async Task InitPubSub()
        {
            TopicName topicName = new TopicName(ProjectId, topicId);
            SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();
            SubscriptionName subscriptionName = new SubscriptionName(ProjectId, SubsciptionId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
            await subscriber.StartAsync((msg,token) =>
            {
                var attributes = msg.Attributes.Values;
                foreach(var attrib in attributes)
                {
                    if(attrib.Contains("metadata"))
                    {
                            return Task.FromResult(SubscriberClient.Reply.Ack);
                    }
                }

                Console.WriteLine("event fired...");
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });
        }
        public async Task SaveToStorage(string jsonData) 
        {
            try 
            {
                var client = await StorageClient.CreateAsync();
                var content = Encoding.UTF8.GetBytes(jsonData);
                await client.UploadObjectAsync(BucketName,
                                                    "data.json",
                                                    "text/plain",
                                                    new MemoryStream(content));
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public async Task StartVault() 
        {
            List<SecretMetadata> metadatas = new List<SecretMetadata>();
            try
            {
                var vaultSettings = new VaultClientSettings(Constants.Host,
                                                            new TokenAuthMethodInfo(Constants.Token));
                var client = new VaultClient(vaultSettings);
                var root = "kv";
                var keys = client.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(path: "",mountPoint: root).Result;
                foreach(var key in keys.Data.Keys) 
                {
                    var keyValue = client.V1.Secrets.KeyValue.V2.ReadSecretMetadataAsync(path: key, mountPoint:root).Result;
                    var versions = keyValue.Data.Versions;
                    foreach(var version in versions) 
                    {
                       var versionNum = int.Parse(version.Key);
                       var isDeleted = !string.IsNullOrWhiteSpace(version.Value.DeletionTime);
                       metadatas.Add(new SecretMetadata 
                       {
                           Name = key,
                           Version = versionNum,
                           IsDeleted = isDeleted
                       });
                    }
                    Console.WriteLine(key);
                }
                var jsonData = JsonConvert.SerializeObject(metadatas);
                await SaveToStorage(jsonData);
                Console.WriteLine("data saved");
                await GetFromStorage();
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc);
            }
            finally
            {
          
            }
          
        }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
            // Task.Run(async () => {
            //     await  StartVault();
            // });

            Task.Run(async () => {
                await InitPubSub();
            });
          
        }
    }
}
