// project01.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "ImageGenerator.h"
#include "resource.h"

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];								// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];								// The title bar text
ImageGenerator *ig=NULL;
HPEN hRpen;
HPEN hGpen;
HPEN hBpen;
HBRUSH hRbrush;
HBRUSH hGbrush;
HBRUSH hBbrush;
ImgGenData data;
UINT	timerId;
bool isBlue;
bool isRed;
bool isGreen;
int	x, y;


// Foward declarations of functions included in this code module:
ATOM				MyRegisterClass(HINSTANCE hInstance);
BOOL				InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK	About(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK	Options(HWND, UINT, WPARAM, LPARAM);
VOID CALLBACK		TimerProc(
  HWND hwnd,     // handle of window for timer messages
  UINT uMsg,     // WM_TIMER message
  UINT idEvent,  // timer identifier
  DWORD dwTime   // current system time
);

void				SetOptionFlds( HWND, HWND );
void				GetOptionFlds( HWND, HWND );

int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
 	// TODO: Place code here.
	MSG msg;
	HACCEL hAccelTable;

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_PROJECT01, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance (hInstance, nCmdShow)) 
	{
		return FALSE;
	}

	hAccelTable = LoadAccelerators(hInstance, (LPCTSTR)IDC_PROJECT01);

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0)) 
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg)) 
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	return msg.wParam;
}



//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
//  COMMENTS:
//
//    This function and its usage is only necessary if you want this code
//    to be compatible with Win32 systems prior to the 'RegisterClassEx'
//    function that was added to Windows 95. It is important to call this function
//    so that the application will get 'well formed' small icons associated
//    with it.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX); 

	wcex.style			= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc	= (WNDPROC)WndProc;
	wcex.cbClsExtra		= 0;
	wcex.cbWndExtra		= 0;
	wcex.hInstance		= hInstance;
	wcex.hIcon			= LoadIcon(hInstance, (LPCTSTR)IDI_PROJECT01);
	wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
	wcex.lpszMenuName	= (LPCSTR)IDC_PROJECT01;
	wcex.lpszClassName	= szWindowClass;
	wcex.hIconSm		= LoadIcon(wcex.hInstance, (LPCTSTR)IDI_SMALL);

	return RegisterClassEx(&wcex);
}

//
//   FUNCTION: InitInstance(HANDLE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   HWND hWnd;
   INITCOMMONCONTROLSEX ctrlsex;
   hInst = hInstance; // Store instance handle in our global variable

//   hWnd = CreateWindow(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
//      CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, NULL, NULL, hInstance, NULL);

   ctrlsex.dwSize = sizeof(INITCOMMONCONTROLSEX);
   ctrlsex.dwICC = ICC_TAB_CLASSES;
   ::InitCommonControlsEx( &ctrlsex );

   hWnd = CreateWindow( szWindowClass, 
						szTitle, 
						WS_OVERLAPPEDWINDOW,
						0, 
						0, 
						XMAX,
						YMAX,
						NULL,
						NULL,
						hInstance,
						NULL);
   if (!hWnd)
   {
      return FALSE;
   }

   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   return TRUE;
}

//
//  FUNCTION: WndProc(HWND, unsigned, WORD, LONG)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int wmId, wmEvent;
	int xPos, yPos;
	PAINTSTRUCT ps;
	HDC hdc;
	char cKey;
	RECT rt;
	MSG	m;
	DWORD err;

	TCHAR szHello[MAX_LOADSTRING];
	LoadString(hInst, IDS_HELLO, szHello, MAX_LOADSTRING);
	switch (message) 
	{
		case WM_CREATE:
			if( !ig ){
				ig = new ImageGenerator( hWnd );
				ig->Init();
			}
			hRpen = CreatePen( PS_SOLID, 10, RGB(255,0,0));
			hGpen = CreatePen( PS_SOLID, 10, RGB(0,255,0));
			hBpen = CreatePen( PS_SOLID, 10, RGB(0,0,255));
			hRbrush = CreateSolidBrush( RGB(255,0,0));
			hGbrush = CreateSolidBrush( RGB(0,255,0));
			hBbrush = CreateSolidBrush( RGB(0,0,255));
			timerId = 0;
			isRed = isGreen = isBlue = false;
			break;
		case WM_KEYDOWN:
			cKey = (char) wParam;  
			if( cKey == ' '){
				ShowWindow( hWnd,SW_MINIMIZE);
				break;
			} else if( cKey == 'T'){
				if(ig ){
					if( timerId == 0 ){
						timerId = SetTimer(hWnd, 0, 50, TimerProc );
					} else {
						if(!KillTimer(hWnd, 0)){
							err = GetLastError();
						}
						timerId = 0;
					}
				}
			}

			if(PeekMessage( &m, hWnd, WM_PAINT,WM_PAINT, PM_NOREMOVE ))
				break;
			if( cKey == 'S'){
				if(ig)
					ig->Snapshot();
			} else if( cKey == 'R'){
				if(ig)
					ig->Refresh();
			} else if( cKey == 'C'){
				if(ig)
					ig->Clear();
			} else if(ig)
				ig->Process();

			GetClientRect(hWnd, &rt);
			InvalidateRect(hWnd, &rt, false);
			break;
		case WM_COMMAND:
			wmId    = LOWORD(wParam); 
			wmEvent = HIWORD(wParam); 
			// Parse the menu selections:
			switch (wmId)
			{
				case IDM_ABOUT:
				   DialogBox(hInst, (LPCTSTR)IDD_ABOUTBOX, hWnd, (DLGPROC)About);
				   break;
				case ID_SETUP_OPTIONS:
				   DialogBox(hInst, (LPCTSTR)IDD_SETUP_DLG, hWnd, (DLGPROC)Options);
				   break;

				case IDM_EXIT:
				   DestroyWindow(hWnd);
				   break;
				default:
				   return DefWindowProc(hWnd, message, wParam, lParam);
			}
			break;
		case WM_PAINT:
			hdc = BeginPaint(hWnd, &ps);
			if(ig)
				ig->Draw();
			EndPaint(hWnd, &ps);
			break;
		case WM_DESTROY:
			if(ig) 
				delete ig;
			DeleteObject( hRpen );
			DeleteObject( hGpen );
			DeleteObject( hBpen );
			DeleteObject( hRbrush );
			DeleteObject( hGbrush );
			DeleteObject( hBbrush );
			PostQuitMessage(0);
			break;
		case WM_MOUSEMOVE:
			xPos = LOWORD(lParam);  // horizontal position of cursor 
			yPos = HIWORD(lParam);  // vertical position of cursor 
			hdc = ::GetDC(hWnd);
			if( isRed || isGreen || isBlue ){
				if( isRed ){
					SelectObject(hdc, hRpen);
				} else if( isBlue ){
					SelectObject(hdc, hGpen);
				} else if( isGreen ){
					SelectObject(hdc, hBpen);
				}
				MoveToEx( hdc, x, y, NULL ); 
				LineTo( hdc, xPos, yPos);
				x=xPos;
				y=yPos;
			}
			break;
		case WM_LBUTTONDOWN:
			x = LOWORD(lParam);  // horizontal position of cursor 
			y = HIWORD(lParam);  // vertical position of cursor 
			isRed = true;
			break;
		case WM_MBUTTONDOWN:
			x = LOWORD(lParam);  // horizontal position of cursor 
			y = HIWORD(lParam);  // vertical position of cursor 
			isGreen = true;
			break;
			break;
		case WM_RBUTTONDOWN:
			x = LOWORD(lParam);  // horizontal position of cursor 
			y = HIWORD(lParam);  // vertical position of cursor 
			isBlue = true;
			break;
		case WM_LBUTTONUP:
			isRed = false;
			break;
		case WM_MBUTTONUP:
			isGreen = false;
			break;
		case WM_RBUTTONUP:
			isBlue = false;
			break;
		case WM_NCMOUSEMOVE:
			isRed = isBlue = isGreen = false;
			break;
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
   }
   return 0;
}

// Mesage handler for about box.
LRESULT CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
		case WM_INITDIALOG:
				return TRUE;

		case WM_COMMAND:
			if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL) 
			{
				EndDialog(hDlg, LOWORD(wParam));
				return TRUE;
			}
			break;
	}
    return FALSE;
}


// Mesage handler for about box.
LRESULT CALLBACK Options(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	HWND hTab;
	tagTCITEMA tc;
	int ix;
	LPNMHDR nm;
	int code;
	char msg[128];

	hTab = GetDlgItem( hDlg, IDC_TAB );

	switch (message)
	{
		case WM_INITDIALOG:
				tc.mask = TCIF_TEXT;
				tc.pszText = "   R";
				tc.cchTextMax = 4;
				tc.iImage = -1;
				ix = TabCtrl_InsertItem( hTab, 0, &tc);
				tc.pszText = "   G";
				ix = TabCtrl_InsertItem( hTab, 1, &tc);
				tc.pszText = "   B";
				ix = TabCtrl_InsertItem( hTab, 2, &tc);
				TabCtrl_SetCurFocus( hTab, 0 );
				data = ig->data;
				SetOptionFlds( hDlg, hTab );
				return TRUE;
		case WM_COMMAND:
			ix = TabCtrl_GetCurFocus(hTab);
			code = HIWORD(wParam);
			switch(LOWORD(wParam)){
				case IDOK:
					ig->data = data;
					break;
				case IDCANCEL:
					break;
				case IDC_ADAPTATION:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_ADAPTATION, msg, 128);  
						data.adaptation[ix] = atof( msg );
					}
					break;
				case IDC_DOMINATION:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_DOMINATION, msg, 128);  
						data.domination[ix] = atof( msg );
					}
					break;
				case IDC_DEATH:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_DEATH, msg, 128);  
						data.death[ix] = atof( msg );
					}
					break;
				case IDC_SURVIVAL:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_SURVIVAL, msg, 128);  
						data.survival[ix] = atof( msg );
					}
					break;
				case IDC_INFLUENCE:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_INFLUENCE, msg, 128);  
						data.influence[ix] = atof( msg );
					}
					break;
				case IDC_MUTATION:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_MUTATION, msg, 128);  
						data.mutation[ix] = atof( msg );
					}
					break;
				case IDC_STEPS:
					if(code == EN_KILLFOCUS){
						GetDlgItemText( hDlg, IDC_STEPS, msg, 128);  
						data.steps = atoi( msg );
					}
					break;
			}
			if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL) 
			{
				EndDialog(hDlg, LOWORD(wParam));
				return TRUE;
			}
			break;
		case WM_NOTIFY:
			nm = (LPNMHDR) lParam;
			switch( (int) wParam){
				case IDC_TAB:
					switch( nm->code ){
					case TCN_SELCHANGING:
						GetOptionFlds( hDlg, hTab );
						break;
					case TCN_SELCHANGE:
						SetOptionFlds( hDlg, hTab );
						SetFocus( GetDlgItem( hDlg, IDOK ));
						break;
					}
					break;
			}
			break;
	}
    return FALSE;
}

void SetOptionFlds( HWND hDlg, HWND hTab ){
	int ix;
	char msg[128];

	ix = TabCtrl_GetCurFocus(hTab);
	sprintf( msg, "%d", data.steps); 
	SetDlgItemText( hDlg, IDC_STEPS, msg);  
	sprintf( msg, "%.2f", data.adaptation[ix]); 
	SetDlgItemText( hDlg, IDC_ADAPTATION, msg);  
	sprintf( msg, "%.2f", data.domination[ix]); 
	SetDlgItemText( hDlg, IDC_DOMINATION, msg);  
	sprintf( msg, "%.2f", data.death[ix]); 
	SetDlgItemText( hDlg, IDC_DEATH, msg);  
	sprintf( msg, "%.2f", data.survival[ix]); 
	SetDlgItemText( hDlg, IDC_SURVIVAL, msg);  
	sprintf( msg, "%.2f", data.influence[ix]); 
	SetDlgItemText( hDlg, IDC_INFLUENCE, msg);  
	sprintf( msg, "%.2f", data.mutation[ix]); 
	SetDlgItemText( hDlg, IDC_MUTATION, msg);  
}

void GetOptionFlds( HWND hDlg, HWND hTab ){
	int ix;
	char msg[128];

	ix = TabCtrl_GetCurFocus(hTab);
	GetDlgItemText( hDlg, IDC_STEPS, msg, 128);  
	data.steps = atoi(msg);
	GetDlgItemText( hDlg, IDC_ADAPTATION, msg, 128);  
	data.adaptation[ix] = atof(msg); 
	GetDlgItemText( hDlg, IDC_DOMINATION, msg, 128);  
	data.domination[ix] = atof(msg); 
	GetDlgItemText( hDlg, IDC_DEATH, msg, 128);  
	data.death[ix] = atof(msg);
	GetDlgItemText( hDlg, IDC_SURVIVAL, msg, 128);  
	data.survival[ix] = atof( msg );
	GetDlgItemText( hDlg, IDC_INFLUENCE, msg, 128);  
	data.influence[ix] = atof(msg);
	GetDlgItemText( hDlg, IDC_MUTATION, msg, 128);  
	data.mutation[ix] = atof(msg);
}

VOID CALLBACK TimerProc(
  HWND hwnd,     // handle of window for timer messages
  UINT uMsg,     // WM_TIMER message
  UINT idEvent,  // timer identifier
  DWORD dwTime   // current system time
  ){
	MSG	m;
	RECT rt;

	if(!PeekMessage( &m, hwnd, WM_PAINT,WM_PAINT, PM_NOREMOVE )){
		ig->Process();
		GetClientRect(hwnd, &rt);
		InvalidateRect(hwnd, &rt, false);
	}

}


