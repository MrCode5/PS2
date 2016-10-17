Imports System.Runtime.InteropServices
Imports SlimDX


Public Class Form1
	'"PlanetSide2_x64.exe"+03448970 + 224
	'or +03449290+224
	Dim FovBaseAddress As String = "PlanetSide2_x64.exe"
	Dim FovOffsets() As UInteger = {&H3448970, &H224}

	'PlanetSide2_x64.exe+32D62A8
	Dim CameraPosBaseAddress As String = "PlanetSide2_x64.exe"
	Dim CameraPosOffsets() As UInteger = {&H32D62A4}

	'"PlanetSide2_x64.exe"+034776F8+2FC
	Dim CameraLookAtBaseAddress As String = "PlanetSide2_x64.exe"
	Dim CameraLookAtOffsets() As UInteger = {&H34776F8, &H300}

	Dim AllPossiblePlayerStrings As String() = {"Armor_"}

	Dim PlayerLoader As New Threading.Thread(AddressOf LoadPlayers)

	Dim PlayersData As New List(Of PlayerData)

	Dim MyPlayer As PlayerData

	Dim MemoryReader As MemoryReader

	Dim CustomGraphics As CustomGraphics

	Declare Function GetWindowRect Lib "user32.dll" (ByVal hWnd As IntPtr, ByRef prmtypRECT As Drawing.Rectangle) As Integer

	Protected Overrides ReadOnly Property CreateParams() As CreateParams
		Get
			Dim cp As CreateParams = MyBase.CreateParams
			cp.ExStyle = cp.ExStyle Or &H80000 Or &H20
			Return cp
		End Get
	End Property
	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		System.Windows.Forms.Cursor.Hide()
		MemoryReader = New MemoryReader("PlanetSide2_x64")
		CustomGraphics = New CustomGraphics(Me.Handle, Me.Width, Me.Height)
		PlayerLoader.Start()
	End Sub

	Sub Main()
		For Each player In PlayersData
			player.Update(MemoryReader)
		Next
		For Each player In PlayersData
			If player.PlayerName = "DankMemes47" Then
				MyPlayer = player
				Exit For
			End If
		Next


		Dim fov As Single = MemoryReader.ReadSingle(MemoryReader.GetAddress(FovBaseAddress, FovOffsets))
		fov *= Math.PI / 180.0F

	
		Dim cameraposaddress As IntPtr = MemoryReader.GetAddress(CameraPosBaseAddress, CameraPosOffsets)
		Dim camerapos As Vector3
		camerapos.X = MemoryReader.ReadSingle(cameraposaddress)
		camerapos.Y = MemoryReader.ReadSingle(cameraposaddress + 4)
		camerapos.Z = MemoryReader.ReadSingle(cameraposaddress + 8)


		Dim cameralookataddress As IntPtr = MemoryReader.GetAddress(CameraLookAtBaseAddress, CameraLookAtOffsets)
		Dim cameralookat As Vector3
		cameralookat.X = MemoryReader.ReadSingle(cameralookataddress)
		cameralookat.Y = MemoryReader.ReadSingle(cameralookataddress + 4)
		cameralookat.Z = MemoryReader.ReadSingle(cameralookataddress + 8)


		Dim ViewMatrix As Matrix = Matrix.LookAtRH(camerapos, camerapos + cameralookat, Vector3.UnitY)
		Dim ProjectionMatrix = Matrix.PerspectiveFovRH(fov, Me.Width / Me.Height, 0.1, 3000)



		CustomGraphics.ViewProjection = ViewMatrix * ProjectionMatrix
		For Each player In PlayersData
			If MyPlayer IsNot Nothing And player IsNot MyPlayer And player.Flags <> 0 Then
				CustomGraphics.Draw3DBox(Color.Red, player.Position, New Vector3(0.8, 2.0, 0.8), 0)
				CustomGraphics.Draw3DString(Color.Red, player.Flags, player.Position)
				CustomGraphics.Draw3DString(Color.Red, player.PlayerClassNameOffset.ToString, player.Position + 3 * Vector3.UnitY)
				CustomGraphics.Draw3DString(Color.Red, player.PlayerClassName.ToString, player.Position + 1 * Vector3.UnitY)
				CustomGraphics.Draw3DString(Color.Red, player.PlayerName, player.Position + 2 * Vector3.UnitY)
			End If
		Next



		CustomGraphics.Present()
	End Sub

	Sub LoadPlayers()
		Dim temp As New List(Of PlayerData)

		Dim ptrs As New List(Of IntPtr)
		For Each possiblestring In AllPossiblePlayerStrings
			ptrs.AddRange(MemoryReader.FindBytes(System.Text.ASCIIEncoding.ASCII.GetBytes(possiblestring)))
		Next


		For Each ptr In ptrs
			Dim str1 As String = MemoryReader.ReadString(ptr + &H5C0)
			Dim str2 As String = MemoryReader.ReadString(ptr + &H620)

			Dim str3 As String = MemoryReader.ReadString(ptr)
			If ValidClassName(str3) And ValidCharacterName(str1) And str1 = str2 Then
				temp.Add(New PlayerData(str1, str3, ptr))
			End If
		Next

		PlayersData = temp
	End Sub
	Function ValidCharacterName(value As String) As Boolean
		If value.Length < 6 Then
			Return False
		End If
		If value.Contains("SilentAssassin4200") Then
			MsgBox(value)
		End If
		For Each charecter In value
			If Char.IsLetterOrDigit(charecter) = False Then
				Return False
			End If
		Next
		Return True
	End Function
	Function ValidClassName(value As String) As Boolean
		If value.Contains("_Look001.adr") = False Then
			Return False
		End If
		Return True
	End Function

	Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
		Main()
	End Sub

	Const padtop As Integer = 27
	Const padother As Integer = 7
	Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
		If PlayerLoader.IsAlive = False Then
			PlayerLoader = New Threading.Thread(AddressOf LoadPlayers)
			PlayerLoader.Start()
		End If

		TopMost = True

		Dim hwnd As IntPtr = Process.GetProcessesByName("PlanetSide2_x64")(0).MainWindowHandle

		Dim windowrectangle As Drawing.Rectangle
		GetWindowRect(hwnd, windowrectangle)

		'windowed
		If True Then
			Me.Location = windowrectangle.Location + New Point(padother, padtop)
			Me.Width = windowrectangle.Width - windowrectangle.X - padother * 2
			Me.Height = windowrectangle.Height - windowrectangle.Y - padother - padtop
		Else

			Me.Location = New Point(0, 0)
			Me.Width = windowrectangle.Width
			Me.Height = windowrectangle.Height
		End If




		CustomGraphics.Resize(Width, Height)
	End Sub
End Class
Enum Faction
	VS = 1
	NC = 2
	TR = 3
End Enum
Enum Stance As UInt32
	Standing = 0
	Crouching = 1
	Walking = 2
	Running = 3
	Falling = 4
	CrouchWalking = 5
	Jumping = 6
End Enum
Enum CharacterFlags
	Alive = 2
	Cloacked = 8
End Enum
Class PlayerData
	Const PositionOffset As Integer = -&H794
	Const StanceOffset As Integer = &H718
	Const FlagsOffset As Integer = &HDD4

	Public PlayerName As String
	Public PlayerClassName As String
	Public PlayerClassNameOffset As IntPtr

	Public Position As Vector3
	Public Faction As Faction

	Public Stance As Stance

	Public Flags As UInt32

	Public Alive As Boolean
	Public Cloacked As Boolean
	Sub New(pName As String, pClass As String, pOffset As IntPtr)
		Me.PlayerName = pName
		Me.PlayerClassName = pClass
		Me.PlayerClassNameOffset = pOffset

		Select Case Split(PlayerClassName, "_")(1)
			Case "VS"
				Faction = ToxicPS2.Faction.VS
			Case "NC"
				Faction = ToxicPS2.Faction.NC
			Case "TR"
				Faction = ToxicPS2.Faction.TR
		End Select
	End Sub
	Sub Update(memoryReader As MemoryReader)
		Dim posx As Single = memoryReader.ReadSingle(PlayerClassNameOffset + PositionOffset)
		Dim posy As Single = memoryReader.ReadSingle(PlayerClassNameOffset + PositionOffset + 4)
		Dim posz As Single = memoryReader.ReadSingle(PlayerClassNameOffset + PositionOffset + 8)
		Me.Position = New Vector3(posx, posy, posz)


		Dim stance As UInt32 = memoryReader.ReadUInt32(PlayerClassNameOffset + StanceOffset)
		Me.Stance = stance

		Dim flags As UInt32 = memoryReader.ReadUInt32(PlayerClassNameOffset + FlagsOffset)

		Me.Flags = flags

		Me.Alive = (flags And CharacterFlags.Alive) <> 0
		Me.Cloacked = (flags And CharacterFlags.Cloacked) <> 0

	End Sub
	Overrides Function ToString() As String
		Return String.Format("Name:{0}, Faction:{1}, Position:{2}, Stance:{3}, Alive:{4}, Cloacked:{5}", PlayerName, Faction.ToString, Position.ToString, Stance.ToString, Alive.ToString, Cloacked.ToString)
	End Function
End Class


Class MemoryReader
	Declare Function ReadProcessMemoryUInt32 Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef lpBuffer As UInt32, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
	Declare Function ReadProcessMemoryUInt64 Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef lpBuffer As UInt64, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
	Declare Function ReadProcessMemoryByte Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef lpBuffer As Byte, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
	Declare Function ReadProcessMemorySingle Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef lpBuffer As Single, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
	Declare Function ReadProcessMemoryByteArray Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer() As Byte, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean




	Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Boolean, ByVal dwProcessId As Integer) As IntPtr
	Const PROCESS_VM_READ = &H10
	Const PROCESS_ALL_ACCESS = &H1F0FFF


	Declare Sub GetSystemInfo Lib "kernel32" (ByRef lpSystemInfo As SYSTEM_INFO)


	Declare Function VirtualQueryEx Lib "kernel32" (hProcess As IntPtr, lpAddress As IntPtr, ByRef lpBuffer As MEMORY_BASIC_INFORMATION, dwLength As UInt32) As UInt32
	Const MEM_COMMIT As Integer = &H1000
	Const PAGE_READWRITE As Integer = &H4


	Dim Process As Process
	Dim ProcessHandle As IntPtr
	Sub New(processname As String)
		Process = Process.GetProcessesByName(processname)(0)
		ProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, False, Process.Id)
	End Sub
	Function ReadUInt32(Address As IntPtr) As UInt32
		Dim returnvalue As UInt32
		ReadProcessMemoryUInt32(ProcessHandle, Address, returnvalue, 4, 0)
		Return returnvalue
	End Function
	Function ReadByte(Address As IntPtr) As Byte
		Dim returnvalue As Byte
		ReadProcessMemoryByte(ProcessHandle, Address, returnvalue, 8, 0)
		Return returnvalue
	End Function
	Function ReadSingle(Address As IntPtr) As Single
		Dim returnvalue As Single
		ReadProcessMemorySingle(ProcessHandle, Address, returnvalue, 4, 0)
		Return returnvalue
	End Function
	Function ReadUInt64(Address As IntPtr) As UInt64
		Dim returnvalue As UInt64
		ReadProcessMemoryUInt64(ProcessHandle, Address, returnvalue, 8, 0)
		Return returnvalue
	End Function
	Function ReadString(Address As IntPtr) As String
		Dim Bytes(64 - 1) As Byte
		Dim StringActualLength As Integer = 64
		For I = 0 To Bytes.Length - 1
			ReadProcessMemoryByte(ProcessHandle, Address + I, Bytes(I), 1, 0)
			If Bytes(I) = 0 Then
				StringActualLength = I
				Exit For
			End If
		Next
		Array.Resize(Bytes, StringActualLength)
		Return System.Text.ASCIIEncoding.ASCII.GetChars(Bytes)
	End Function
	Function FindBytes(Bytes() As Byte) As IntPtr()
		Dim systeminfo As SYSTEM_INFO
		GetSystemInfo(systeminfo)

		Dim Addresses As New List(Of IntPtr)


		Dim i As ULong = systeminfo.minimumApplicationAddress
		Do While i < CLng(systeminfo.maximumApplicationAddress)
			Dim membasicinfo As New MEMORY_BASIC_INFORMATION
			Dim membasicinfosize As Int32 = Marshal.SizeOf(membasicinfo)
			Dim result As Integer = VirtualQueryEx(ProcessHandle, i, membasicinfo, membasicinfosize)
			If result = 0 Then
				Dim errorcode As Integer = Marshal.GetLastWin32Error()
				MessageBox.Show(errorcode.ToString)
			End If
			If membasicinfo.Protect = PAGE_READWRITE And membasicinfo.State = MEM_COMMIT Then
				Dim ptrs() As IntPtr = FindBytesInRegion(Bytes, membasicinfo.BaseAddress, membasicinfo.RegionSize)
				If ptrs.Length > 0 Then
					Addresses.AddRange(ptrs)
				End If
			End If
			i += CLng(membasicinfo.RegionSize)
		Loop


		Return Addresses.ToArray

	End Function
	Function FindBytesInRegion(bytes() As Byte, baseaddress As IntPtr, regionsize As Integer) As IntPtr()
		Dim dump(regionsize - 1) As Byte
		ReadProcessMemoryByteArray(ProcessHandle, baseaddress, dump, dump.Length, 0)



		Dim addresses As New List(Of IntPtr)

		For i = 0 To regionsize - bytes.Length - 1 Step 1
			For j = 0 To bytes.Length - 1
				Dim x As Byte = dump(i + j)
				Dim y As Byte = bytes(j)

				If (x = y) = False Then
					Exit For
				ElseIf j = bytes.Length - 1 Then
					addresses.Add(baseaddress + i)
				End If
			Next

		Next

		Return addresses.ToArray
	End Function
	Function GetAddress(ByRef AddressBase As String, ByVal Offsets As UInteger()) As IntPtr
		Dim ProcessModule As ProcessModule = GetModuleByName(AddressBase)
		Dim BaseAddress As IntPtr = ProcessModule.BaseAddress + Offsets(0)

		If Offsets.Count > 1 Then
			For i = 1 To Offsets.Count - 1
				BaseAddress = ReadUInt32(BaseAddress) + Offsets(i)
			Next
		End If

		Return BaseAddress
	End Function
	Function GetModuleByName(moduleName As String) As ProcessModule
		For Each ProcessModule In Process.Modules
			If ProcessModule.ModuleName = moduleName Then
				Return ProcessModule
			End If
		Next
		Return Nothing
	End Function

	Public Structure MEMORY_BASIC_INFORMATION
		Public BaseAddress As IntPtr
		Public AllocationBase As IntPtr
		Public AllocationProtect As UInteger
		Public RegionSize As IntPtr
		' size of the region allocated by the program
		Public State As UInteger
		' check if allocated (MEM_COMMIT)
		Public Protect As UInteger
		' page protection (must be PAGE_READWRITE)
		Public lType As UInteger
	End Structure
	Public Structure SYSTEM_INFO
		Public processorArchitecture As UShort
		Private reserved As UShort
		Public pageSize As UInteger
		Public minimumApplicationAddress As IntPtr
		' minimum address
		Public maximumApplicationAddress As IntPtr
		' maximum address
		Public activeProcessorMask As IntPtr
		Public numberOfProcessors As UInteger
		Public processorType As UInteger
		Public allocationGranularity As UInteger
		Public processorLevel As UShort
		Public processorRevision As UShort
	End Structure
End Class