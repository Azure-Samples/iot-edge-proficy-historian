using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Azure.IoT.Gateway;
using Proficy.Historian.Module;
using System.Text;
using System.Collections.Generic;
using Proficy.Historian.ClientAccess.API;

namespace Proficy.Historian.Module.Tests
{
    [TestClass]
    public class ModuleTest
    {
        static string ServerName = Properties.Settings.Default.ServerName;
        static string UserName = Properties.Settings.Default.UserName;
        static string Password = Properties.Settings.Default.Password;
        static string TagName = Properties.Settings.Default.TagName;

        static Broker _testBroker;
        static GatewayModule _testModule;
  
        [TestMethod]
        public void CreateMockGateway()
        {
            try
            {
                _testBroker = new Broker(1, 1);
            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to initialize gateway mock: " + ex.Message);
            }
            
        }

        [TestMethod]
        public void CreateModule()
        {
            try
            {
                _testModule = new GatewayModule();
            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to create module: " + ex.Message);
            }
        }

        [TestMethod]
        public void InitializeModule()
        {
            try
            {
                string jsonConfiguration = @"{
                    'ServerName': '" + ServerName + @"',
                    'UserName': '" + UserName + @"',
                    'Password': '" + Password + @"',
                    'TagsToSubscribe': [
                        { 'TagName': '" + TagName + @"' }
                    ]
                }";
                byte[] configuration = Encoding.UTF8.GetBytes(jsonConfiguration);

                _testModule.Create(_testBroker, configuration);

                
            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to initialize module: " + ex.Message);
            }
        }

        [TestMethod]
        public void StartModule()
        {
            try
            {
                _testModule.Start();


            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to start module: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Failed to Publish Message.")]
        public void SimulateDataChange()
        {
            CurrentValue dummyDataChangeEvent = new CurrentValue();
            dummyDataChangeEvent.Tagname = TagName;
            dummyDataChangeEvent.Value = 1;
            dummyDataChangeEvent.Time = DateTime.Now;
            dummyDataChangeEvent.Quality = DataQuality.Good;

            List<CurrentValue> dummyValues = new List<CurrentValue>();
            dummyValues.Add(dummyDataChangeEvent);

            _testModule.Historian_DataChangedEvent(dummyValues);
        }

        [TestMethod]
        public void Cleanup()
        {
            try
            {
                _testModule.Destroy();
            }
            catch (Exception ex)
            {
                Assert.Fail("Unable to close module: " + ex.Message);
            }
        }
    }
}
