﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Divisionsmatch
{
    /// <summary>
    /// dialog til at udpege o-service fil med tidligere resultater
    /// </summary>
    public partial class frmÅbnDivisionsResultat : Form
    {
        /// <summary>
        /// creator
        /// </summary>
        public frmÅbnDivisionsResultat()
        {
            InitializeComponent();
        }

        /// <summary>
        /// egenskab med reference til tidligere resultarter
        /// </summary>
        public DivisionsResultat.DivisionsResultat DivisionsResultat { get; internal set; }
        
        /// <summary>
        /// egenskab med reference til stævnet
        /// </summary>
        public Staevne Staevne { get; set; }
        
        /// <summary>
        /// egenskab for folderen med resultater
        /// </summary>
        public string InitialDirectory { get; set; }

        private void frmDivisionsResultat_Load(object sender, EventArgs e)
        {
        }

        private void btnOpenFileXML_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "O-service Divisionsresultat (*.xml)|*.xml";
            openFile.CheckPathExists = true;
            openFile.AddExtension = true;
            openFile.DefaultExt = ".xml";
            openFile.SupportMultiDottedExtensions = true;
            openFile.Title = "Åbn divisionsresultat";
            openFile.Multiselect = false;
            if (InitialDirectory != string.Empty)
            {
                openFile.InitialDirectory = InitialDirectory;
            }
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // load file
                try
                {
                    DivisionsResultat = null;
                    buttonOK.Enabled = false;

                    txtXMLFile.Text = openFile.FileName;

                    DivisionsResultat.DivisionsResultat mitDivisionsResultat = Util.OpenDivisionsResultat(openFile.FileName);
                    if (mitDivisionsResultat != null)
                    {
                        // fyld dialogen
                        this.textBoxÅr.Text = mitDivisionsResultat.År.ToString();
                        if (mitDivisionsResultat.Division < 7)
                        {
                            this.textBoxDivision.Text = mitDivisionsResultat.Division.ToString();
                        }
                        this.textBoxKreds.Text = mitDivisionsResultat.Kreds.Navn.ToString();
                        this.textBoxKredsId.Text = mitDivisionsResultat.Kreds.Id.ToString();
                        this.listBoxMatcher.Items.Clear();
                        foreach (var m in mitDivisionsResultat.DivisionsMatchResultater.OrderBy(item => item.Runde).Where(item => item.Runde < mitDivisionsResultat.DivisionsMatchResultater.Count))
                        {
                            this.listBoxMatcher.Items.Add(string.Format("{0} - {1} - {2}", m.Runde, m.Dato, m.Skov));
                        }

                        if (mitDivisionsResultat.DivisionsMatchResultater.Count > 1)
                        {
                            foreach (var k in mitDivisionsResultat.DivisionsMatchResultater[0].Klubber)
                            {
                                this.listBoxDiviKlubber.Items.Add(k.Navn);
                            }
                        }

                        buttonOK.Enabled = true;

                        DivisionsResultat.DivisionsMatchResultat denneMatch = mitDivisionsResultat.DivisionsMatchResultater.OrderBy(item => item.Runde).Last();

                        // fyld data for denne match
                        this.dateTimePicker1.Value = DateTime.Parse(denneMatch.Dato);
                        if (mitDivisionsResultat.Division < 7)
                        {
                            this.textBoxStaevneDivision.Text = mitDivisionsResultat.Division.ToString();
                        }
                        this.textBoxStaevneKreds.Text = mitDivisionsResultat.Kreds.Navn.ToString();
                        this.textBoxStaevneKredsId.Text = mitDivisionsResultat.Kreds.Id.ToString();

                        this.textBoxStaevneSkov.Text = denneMatch.Skov;
                        this.textBoxStaevneType.Text = mitDivisionsResultat.Division == 8 ? "Op/Ned" : mitDivisionsResultat.Division == 9 ? "Finale" : "Divisionsmatch";
                        this.textBoxStaevneRunde.Text = denneMatch.Runde.ToString();
                        this.textBoxBeskrivelse.Text = denneMatch.Beskriv;

                        foreach (var k in denneMatch.Klubber)
                        {
                            listBoxMatchKlubber.Items.Add(k);
                        }


                        DivisionsResultat = mitDivisionsResultat;
                        //MessageBox.Show("Tidligere resultater er indlæst. Tryk OK for at benytte dem \n");
                    }
                    else
                    {
                        MessageBox.Show("Tidligere resultater kunnne ikke åbnes \n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Tidligere resultater kunnne ikke åbnes \n" + ex.Message + "\n" + ex.StackTrace, "Fejl");
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // inital data fra o-service
            // opret config data for stævnet vi benytter o-service Id i stævnet
            DivisionsResultat.DivisionsMatchResultat denneMatch = DivisionsResultat.DivisionsMatchResultater.OrderBy(item => item.Runde).Last();

            Staevne.Config.Dato = DateTime.Parse(denneMatch.Dato);
            Staevne.Config.Skov = denneMatch.Skov;
            Staevne.Config.Type = DivisionsResultat.Division == 8 ? "Op/Ned" : DivisionsResultat.Division == 9 ? "Finale" : "Divisionsmatch";
            Staevne.Config.Division = DivisionsResultat.Division;
            Staevne.Config.Kreds = DivisionsResultat.Kreds.Navn;
            Staevne.Config.KredsId = DivisionsResultat.Kreds.Id;
            Staevne.Config.Beskrivelse = denneMatch.Beskriv;
            Staevne.Config.Runde = denneMatch.Runde;
            Staevne.Config.DivisionsResultatFil = this.txtXMLFile.Text;

            // opret klubberne i stævnet
            foreach (var rk in denneMatch.Klubber)
            {
                Klub klub = new Klub();
                klub.Id = new KlubId(rk.Id.Id,rk.Id.Type);
                klub.Navn = rk.Navn;
                klub.NavnKort = rk.NavnKort;
                klub.Klubber = new List<Klub>();
                foreach (var k in rk.Klubber)
                {
                    Klub newKlub = new Klub() { Id = new KlubId(k.Id.Id, k.Id.Type), Navn = k.Navn, NavnKort = k.NavnKort };
                    klub.Klubber.Add(newKlub);
                }
                Staevne.Config.Klubber.Add(klub);
            }

            // fjerne denneMatch fra DivisionsResultat
            DivisionsResultat.DivisionsMatchResultater.Remove(denneMatch);
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
