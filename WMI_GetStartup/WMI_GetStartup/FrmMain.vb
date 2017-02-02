Imports System.Management
Imports System.IO
Imports Shell32
Public Class FrmMain
    Const quote As String = """"
    Private _AllUsersStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\"
    Private _CurrentUserStartup As String = Environment.GetFolderPath(Environment.SpecialFolder.Programs) &
        "\Startup\"
    Sub New()

        InitializeComponent()
        LvSet()

    End Sub
    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim m As New ManagementClass("Win32_StartupCommand")
        Dim mc As ManagementObjectCollection = m.GetInstances()
        For Each x In mc
            Dim s() As String = {x("Caption").ToString,
                x("Command").ToString, GetPath(x("Command").ToString)}
            LV.Items.Add(New ListViewItem(s))
        Next
    End Sub
    Private Sub LvSet()
        LV.View = View.Details
        LV.FullRowSelect = True
        LV.MultiSelect = False
        LV.Columns.Add("Cpation", 100)
        LV.Columns.Add("Command", 250)
        LV.Columns.Add("Image Path", 250)
    End Sub

    Private Function GetPath(Command As String) As String
        If (Command.IndexOf(".exe")) > 0 Then
            Command = Command.Replace(quote, "")
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
            Command = Command.Replace(quote, "")
            Command = Command.Replace("/", "")
            Command = Command.Replace("-", "")
            Return Command
        End If

    End Function

    Private Function GetShortcutPath(Command As String)
        Dim pathOnly As String = System.IO.Path.GetDirectoryName(Command)
        Dim fileNameOnly As String = System.IO.Path.GetFileName(Command)


        Dim shell As New Shell()
        Dim folder As Folder = shell.NameSpace(pathOnly)
        Dim folderItem As FolderItem = folder.ParseName(fileNameOnly)
        If (folderItem IsNot Nothing) Then
            Dim link As ShellLinkObject = DirectCast(folderItem.GetLink, ShellLinkObject)
            Return link.Path
        End If
        Return String.Empty
    End Function
End Class