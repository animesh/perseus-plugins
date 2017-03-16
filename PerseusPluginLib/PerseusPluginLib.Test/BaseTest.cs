﻿using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerseusPluginLib.Test
{
    public class BaseTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Assembly assembly = Assembly.GetAssembly(GetType());

            AppDomainManager manager = new AppDomainManager();
            FieldInfo entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
            entryAssemblyfield.SetValue(manager, assembly);

            AppDomain domain = AppDomain.CurrentDomain;
            FieldInfo domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
            domainManagerField.SetValue(domain, manager);
        }
    }
}