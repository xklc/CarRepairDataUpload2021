// md5.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "md5.h"
#include "md51.h"


// 这是导出函数的一个示例。
MD5_API const char * md5(const char * encrypt)
{
	static char szRet[256];
	string salt = "www.rongtone.cn";
	printf("in param: %s", encrypt);
	string newkey = salt + encrypt;
	MD5 md5;
	md5.update(newkey);
	strcpy_s(szRet, md5.toString().c_str());
	printf("out param: %s", szRet);
	return szRet;
}

