// ImageGenerator.cpp: implementation of the ImageGenerator class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "ImageGenerator.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

void
ImageGenerator::Init()
{
	HDC hdc;
	if(mInit)
		return;

	Refresh();

	hdc = GetDC( mHwnd );
	mbmp = CreateCompatibleBitmap( hdc,XMAX, YMAX);
	ReleaseDC( mHwnd, hdc );

	mInit = true;
}

void
ImageGenerator::Refresh()
{
	int r, g, b;
	int row, col;
	RECT rt;

	GetClientRect(mHwnd, &rt);
	srand( (unsigned)time( NULL ) );
	
	for(col = 0; col < YMAX; col++){
		for( row = 0; row < XMAX; row++){
			r = rand() % 256;
			g = rand() % 256;
			b = rand() % 256;
			mMap[iMap][col][row] = RGB(r,g,b);
			imgData[col][row][0] = b;
			imgData[col][row][1] = g;
			imgData[col][row][2] = r;
		}
	}
}

void
ImageGenerator::Clear()
{
	int r, g, b;
	int row, col;
	RECT rt;

	GetClientRect(mHwnd, &rt);
	srand( (unsigned)time( NULL ) );
	
	for(col = 0; col < YMAX; col++){
		for( row = 0; row < XMAX; row++){
			mMap[iMap][col][row] = RGB(0,0,0);
			imgData[col][row][0] = 0;
			imgData[col][row][1] = 0;
			imgData[col][row][2] = 0;
		}
	}

}

void 
ImageGenerator::Process()
{
	int r, g, b, tr, tg, tb;
	int x,y, xm, xp, ym, yp, ix,iy;
	int h,w, i;
	COLORREF color[3][3];

	h = YMAX;
	w = XMAX;

	for( i = 0; i < data.steps; i++){
		for(y = 0; y < h; y++){
			for( x = 0; x < w; x++){
				xm = ((x+w+w)-1)%w; // trick to wrap around x coordinate
				xp = ((x+w)+1)%w; // same other direction
				ym = ((y+h+h)-1)%h; // ...
				yp = ((y+h)+1)%h; // ...

				r = g = b = tr = tg = tb = 0;

				// read in colors from neighbouring pixels
				color[0][0] = mMap[iMap][xm][ym]; 
				color[0][1] = mMap[iMap][xm][y]; 
				color[0][2] = mMap[iMap][xm][yp]; 
				color[1][0] = mMap[iMap][x][ym]; 
				color[1][1] = RGB(0,0,0); 
				color[1][2] = mMap[iMap][x][yp]; 
				color[2][0] = mMap[iMap][xp][ym]; 
				color[2][1] = mMap[iMap][xp][y]; 
				color[2][2] = mMap[iMap][xp][yp]; 

				// some up the color channels
				for( ix = 0; ix < 3; ix++){
					for( iy = 0; iy < 3; iy++){
						tr += GetRValue(color[ix][iy]); 
						tg += GetGValue(color[ix][iy]);
						tb += GetBValue(color[ix][iy]); 
					}
				}

				// assignment not necessary but our new rgb raw values are the previously 
				// calculated totals, eg. could have used t variables directly for further processing
				r = tr;
				g = tg;
				b = tb;

				// the magic happens
				Calculate( r, g, b);

				mMap[(iMap + 3)%2][x][y] = RGB(r,g,b);
				imgData[y][x][0] = b;
				imgData[y][x][1] = g;
				imgData[y][x][2] = r;
			}
		}
		iMap = (iMap + 3)%2;
	}

}

void
ImageGenerator::Draw(){
	int h,w,x,y;
	DWORD err;
	char msg[128];
	int lines;
	BITMAP bInfo;
	BITMAPINFO bI;

	mHdc[0] = GetDC( mHwnd );

	lines = GetObject( mbmp, sizeof(BITMAP), &bInfo );

	h = YMAX;
	w = XMAX;

	if( !mbmp )
		mbmp = CreateCompatibleBitmap( mHdc[0],XMAX, YMAX);

	SetBitmapBits( mbmp, 400*1600, imgData);
	SelectObject(mHdc[1], mbmp);

	if( !BitBlt( mHdc[0],
				 0,
				 0,
				 XMAX,
				 YMAX,
				 mHdc[1],
				 0,
				 0,
				 SRCCOPY)){
		err = GetLastError();
	}
	if(iMap){
		SelectObject(mHdc[0], hGpen);
		SelectObject(mHdc[0], hGbrush);
	} else {
		SelectObject(mHdc[0], hRpen);
		SelectObject(mHdc[0], hRbrush);
	}
//	Ellipse( mHdc[0], 0, 0, 5, 5);
	ReleaseDC( mHwnd, mHdc[0] );
}

void
ImageGenerator::Snapshot()
{
	RECT rt;
	int h,w,x,y;
	
	GetClientRect(mHwnd, &rt);
	mHdc[0] = GetDC( mHwnd );

	h = YMAX;
	w = XMAX;

	for(y = 0; y < h; y++){
		for( x = 0; x < w; x++){
			mMap[iMap][x][y] = GetPixel( mHdc[0], x, y);
			imgData[y][x][2] = GetRValue(mMap[iMap][x][y]);
			imgData[y][x][1] = GetGValue(mMap[iMap][x][y]);
			imgData[y][x][0] = GetBValue(mMap[iMap][x][y]);
		}
	}
	ReleaseDC( mHwnd, mHdc[0] );
}

void
ImageGenerator::Calculate(int &r, int &g, int &b)
{
	double dr, dg, db;


	dr = (double) r;
	dg = (double) g;
	db = (double) b;

	// adaptation
	if( data.adaptation[0] != 0 )
		dr += data.adaptation[0]*(rand()%20);
	if( data.adaptation[1] != 0 )
		dg += data.adaptation[1]*(rand()%20);
	if( data.adaptation[2] != 0 )
		db += data.adaptation[2]*(rand()%20);

	// domination
	if( data.domination[0] != 0 )
		(r>b && r>g)?(dr+=data.domination[0]):(dr-=data.domination[0]);
	if( data.domination[1] != 0 )
		(g>b && g>r)?(dg+=data.domination[1]):(dg-=data.domination[1]);
	if( data.domination[2] != 0 )
		(b>r && b>g)?(db+=data.domination[2]):(db-=data.domination[2]);
	
	// the 2000 and 250 seem to be an arbtirary values
	// not that rgb can be well above 255 cos we use total numbers
	// death
	if(data.death[0] != 0)
		(r>2000 && b<250 && g<250)?(dr-=data.death[0]):(dr+=data.death[0]);
	if(data.death[1] != 0)
		(g>2000 && r<250 && b<250)?(dg-=data.death[1]):(dg+=data.death[1]);
	if(data.death[2] != 0)
		(b>2000 && g<250 && r<250)?(db-=data.death[2]):(db+=data.death[2]);

	// survival
	if(data.survival[0]!=0)
		(r<2000 && (b>0 || g>0))?(dr+=data.survival[0]):(dr-=data.survival[0]);
	if(data.survival[1]!=0)
		(g<2000 && (r>0 || b>0))?(dg+=data.survival[1]):(dg-=data.survival[1]);
	if(data.survival[2]!=0)
		(b<2000 && (g>0 || r>0))?(db+=data.survival[2]):(db-=data.survival[2]);
	
	// influence
	if(data.influence[0]!=0)
		(r==b&&r>0)?(dg+=data.influence[1]):(dg-=data.influence[1]);
	if(data.influence[1]!=0)
		(b==g&&b>0)?(dr+=data.influence[0]):(dr-=data.influence[0]);
	if(data.influence[2]!=0)
		(g==r&&g>0)?(db+=data.influence[2]):(db-=data.influence[2]);

	// mutation
	switch( rand()%100 ){
		case 0:
			if(data.mutation[0]!=0)
				dr += (double)((rand()%20) * data.mutation[0]);
			break;
		case 1:
			if(data.mutation[1]!=0)
				dg += (double)((rand()%20) * data.mutation[1]);
			break;
		case 2:
			if(data.mutation[2]!=0)
				db += (double)((rand()%20) * data.mutation[2]);
			break;
	}

	// wtf was i thinking?? but cool

	// bit shifting by 8 bits, dunno why i did this, it is equiv with a devision by 8
	// bit shifting is fast
	// i quess the numbers gets to large and i am only interested changed information
	r = (int)dr>>3;
	g = (int)dg>>3;
	b = (int)db>>3;

	// turn the rgb values into valid range 
	// could have used mod operator as well
	r<0?r=0:r;
	r>255?r=255:r;
	b<0?b=0:b;
	b>255?b=255:b;
	g<0?g=0:g;
	g>255?g=255:g;

}