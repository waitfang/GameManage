/**
���ܣ�SP_GetUserInfo =����ѯ�û��б�
������@USERID���û��˺�
�����ˣ�wait
ʾ����execute SP_GetUserInfo 1 
**/

CREATE PROCEDURE SP_GetUserInfo
 @USERID int
AS
BEGIN  
	SELECT * INTO #TEMP FROM USERINFO A 
	SELECT * FROM #TEMP
END 