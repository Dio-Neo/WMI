Imports System.Management
Imports System.IO
Imports Shell32
Imports System.Security.Cryptography.X509Certificates
Imports System.Windows.Forms
Public Class FrmMain
    Const quote As String = """"
    Private _AllUsersStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\"
    Private _CurrentUserStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Programs) &
        "\Startup\"
    Sub New()

        InitializeComponent()
        LvSet()
        Width = LV.Columns(0).Width + LV.Columns(1).Width + LV.Columns(2).Width + 50
    End Sub
    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim WaitForm As Form = FrmWait
        WaitForm.Show()
        Application.DoEvents()
        GetStartup()
        Application.DoEvents()
        WaitForm.Close()
    End Sub
    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshToolStripMenuItem.Click
        GetStartup()
    End Sub
    Private Sub GetStartup()
        LV.Items.Clear()
        Dim m As New ManagementClass("Win32_StartupCommand")
        Dim mc As ManagementObjectCollection = m.GetInstances()
        For Each x In mc
            Dim s() As String = {x("Caption").ToString, GetPath(x("Command").ToString),
                                Signature(GetPath(x("Command").ToString))}

            LV.Items.Add(New ListViewItem(s))
            If (s(2) = "Unsigned") Then
                LV.Items(LV.Items.Count - 1).BackColor = Color.Pink
            End If
        Next
    End Sub
    Private Sub LvSet()
        LV.View = View.Details
        LV.FullRowSelect = True
        LV.MultiSelect = False
        LV.Columns.Add("Cpation", 200)
        LV.Columns.Add("Image Path", 500)
        LV.Columns.Add("Signed", 100)
    End Sub

    Private Function GetPath(Command As String) As String
        If (Command.IndexOf(".exe")) > 0 Then
            Command = Command.Replace(quote, String.Empty)
            Command = Command.Substring(0, Command.IndexOf(".exe") + 1)
            Return Command + "exe"

        ElseIf (Command.IndexOf(".lnk") > 0) Then

            If (File.Exists(_AllUsersStartup & Command)) Then
                Command = _AllUsersStartup & Command
                Return GetShortcutPath(Command)
            Else
                Command = _CurrentUserStartup & "\" & Command
                Return GetShortcutPath(Command)
            End If

        Else
            Command = Command.Replace(quote, String.Empty)
            Command = Command.Replace("/", String.Empty)
            Command = Command.Replace("-", String.Empty)
            Return Command
        End If

    End Function

    Private Function GetShortcutPath(Command As String)
        Dim pathOnly As String = Path.GetDirectoryName(Command)
        Dim fileNameOnly As String = Path.GetFileName(Command)
        Dim shell As New Shell()
        Dim folder As Folder = shell.NameSpace(pathOnly)
        Dim folderItem As FolderItem = folder.ParseName(fileNameOnly)
        If (folderItem IsNot Nothing) Then
            Dim link As ShellLinkObject = DirectCast(folderItem.GetLink, ShellLinkObject)
            Return link.Path
        End If
        Return String.Empty
    End Function

    Private Function Signature(Path As String) As String
        Dim theCertificate As X509Certificate2
        Dim chainIsValid As Boolean = False
        Dim theCertificateChain = New X509Chain()

        Try
            Dim theSigner As X509Certificate = X509Certificate.CreateFromSignedFile(Path)
            theCertificate = New X509Certificate2(theSigner)
        Catch ex As Exception
            Return "Unsigned"
        End Try

        theCertificateChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot
        theCertificateChain.ChainPolicy.RevocationMode = X509RevocationMode.Online
        theCertificateChain.ChainPolicy.UrlRetrievalTimeout = New TimeSpan(0, 1, 0)
        theCertificateChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag
        chainIsValid = theCertificateChain.Build(theCertificate)

        If chainIsValid Then
            Return "Signed"
        Else
            Return "Unsigned"
        End If
    End Function


End Class