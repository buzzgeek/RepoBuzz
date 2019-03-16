// ImageGenerator.h: interface for the ImageGenerator class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_IMAGEGENERATOR_H__AD264731_8EDA_4640_B2DD_942462785648__INCLUDED_)
#define AFX_IMAGEGENERATOR_H__AD264731_8EDA_4640_B2DD_942462785648__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
#include "StdAfx.h"

#define	XMAX			400
#define YMAX			400
#define STEPS			9

struct ImgGenData{
	double	adaptation[3];
	double	domination[3];
	double	death[3];
	double	survival[3];
	double	influence[3];
	double	mutation[3];
	int		steps;

	ImgGenData &operator=( const ImgGenData &ig){
		for( int i=0; i<3; i++){
			adaptation[i]	= ig.adaptation[i];
			domination[i]	= ig.domination[i]; 
			death[i]		= ig.death[i];
			survival[i]		= ig.survival[i]  ;
			influence[i]	= ig.influence[i];
			mutation[i]		= ig.mutation[i];
			steps = ig.steps;
		}

		return *this;
	}
};

class ImageGenerator  
{
private:
	HWND	mHwnd;
	HDC		mHdc[2];
	bool	mInit;
	HPEN hRpen;
	HPEN hGpen;
	HPEN hBpen;
	HBRUSH hRbrush;
	HBRUSH hGbrush;
	HBRUSH hBbrush;
	int		ra;
	int		ga;
	int		ba;
	int		iMap;

	COLORREF	mMap[2][XMAX][YMAX];
	HBITMAP		mbmp;
	BITMAPINFO	mBMBInfo;

	virtual void Calculate( int &r, int &g, int &b);

public:
	ImgGenData		data;
	unsigned char	imgData[400][400][4];

	ImageGenerator( HWND aHwnd ){
		mHwnd = aHwnd;
		mInit = false;
		mbmp = 0;
		hRpen = CreatePen( PS_SOLID, 0, RGB(255,0,0));
		hGpen = CreatePen( PS_SOLID, 0, RGB(0,255,0));
		hBpen = CreatePen( PS_SOLID, 0, RGB(0,0,255));
		hRbrush = CreateSolidBrush( RGB(255,0,0));
		hGbrush = CreateSolidBrush( RGB(0,255,0));
		hBbrush = CreateSolidBrush( RGB(0,0,255));
		ra = 1;
		ga = 1;
		ba = 1;
		iMap = 0;
		data.steps = 1;

		for( int i=0; i<3; i++){
			data.adaptation[i] = 0.0;
			data.domination[i] = 10.0;
			data.influence[i] = 10.0;
			data.mutation[i] = 0.0;
			data.death[i] = 10.0;
			data.survival[i] = -5.0;
		}
		mHdc[0] = GetDC( mHwnd );
		mHdc[1] = CreateCompatibleDC( mHdc[0] );
		ReleaseDC( mHwnd, mHdc[0] );

	}
	virtual ~ImageGenerator(){
		DeleteObject( mbmp );
		DeleteDC(mHdc[1]);
		ReleaseDC( mHwnd, mHdc[0] );

		DeleteObject( hRpen );
		DeleteObject( hGpen );
		DeleteObject( hBpen );
		DeleteObject( hRbrush );
		DeleteObject( hGbrush );
		DeleteObject( hBbrush );
	}

	virtual void Init();
	virtual void Process();
	virtual void Draw();
	virtual void Snapshot();
	virtual void Refresh();
	virtual void Clear();

};

#endif // !defined(AFX_IMAGEGENERATOR_H__AD264731_8EDA_4640_B2DD_942462785648__INCLUDED_)
