
// tianya2021.h : PROJECT_NAME Ӧ�ó������ͷ�ļ�
//

#pragma once

#ifndef __AFXWIN_H__
	#error "�ڰ������ļ�֮ǰ������stdafx.h�������� PCH �ļ�"
#endif

#include "resource.h"		// ������


// Ctianya2021App: 
// �йش����ʵ�֣������ tianya2021.cpp
//

class Ctianya2021App : public CWinApp
{
public:
	Ctianya2021App();

// ��д
public:
	virtual BOOL InitInstance();

// ʵ��

	DECLARE_MESSAGE_MAP()
};

extern Ctianya2021App theApp;