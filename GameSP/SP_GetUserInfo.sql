/**
功能：SP_GetUserInfo =》查询用户列表
参数：@USERID：用户账号
创建人：wait
示例：execute SP_GetUserInfo 1 
**/

CREATE PROCEDURE SP_GetUserInfo
 @USERID int
AS
BEGIN  
	SELECT * INTO #TEMP FROM USERINFO A 
	SELECT * FROM #TEMP
END 