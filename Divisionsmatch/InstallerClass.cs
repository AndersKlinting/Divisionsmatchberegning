﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.IO;

namespace OffLine.Installer
{
    /// <summary>
    /// Taken from:http://msdn2.microsoft.com/en-us/library/
    /// system.configuration.configurationmanager.aspx
    /// Set 'RunInstaller' attribute to true.
    /// </summary>
    [RunInstaller(true)]
    public class InstallerClass : System.Configuration.Install.Installer
    {
        /// <summary>
        /// creator
        /// </summary>
        public InstallerClass()
            : base()
        {
            // Attach the 'Committed' event.
            this.Committed += new InstallEventHandler(MyInstaller_Committed);
            // Attach the 'Committing' event.
            this.Committing += new InstallEventHandler(MyInstaller_Committing);
        }

        // Event handler for 'Committing' event.
        private void MyInstaller_Committing(object sender, InstallEventArgs e)
        {
            //Console.WriteLine("");
            //Console.WriteLine("Committing Event occurred.");
            //Console.WriteLine("");
        }

        // Event handler for 'Committed' event.
        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName
                (Assembly.GetExecutingAssembly().Location));
                Process.Start(Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + "\\Divisionsmatch.exe");
            }
            catch
            {
                // Do nothing... 
            }
        }

        /// <summary>
        /// Override the 'Install' method.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }

        /// <summary>
        /// Override the 'Commit' method.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        /// <summary>
        /// Override the 'Rollback' method.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }
    }
}