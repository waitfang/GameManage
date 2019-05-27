//获取地址栏传递的参数信息
function GetUrlRequest() {
    var url = location.search; //获取url中"?"符后的字串
    var theRequest = new Object();
    if (url.indexOf("?") != -1) {
        var str = url.substr(1);
        strs = str.split("&");
        for (var i = 0; i < strs.length; i++) {
            theRequest[strs[i].split("=")[0]] = unescape(strs[i].split("=")[1]);
        }
    }
    return theRequest;

}

var reqObj = GetUrlRequest();
var wsObject = function () {
    var wsObj = null;
    var readyState = new Array("已建立连接", "已关闭连接");
    var state = readyState[1];
    var tidObj = {};

    this.connect = function () {
        try {
            reqObj["token"] = "042CABF8-A62C-48D6-8E09-F8B3FE8BBA20";
            //var host = "ws://localhost:6388/" + reqObj["token"] + "/";
            var host = "ws://127.0.0.1:6388/" + reqObj["token"] + "/";
            wsObj = new WebSocket(host);
             
            wsObj.onmessage = function (msg) {
                var recObj = $.parseJSON(msg.data);

                if (!recObj.result) return;

                if (tidObj[recObj.tid]) {
                    tidObj[recObj.tid](recObj);
                    delete tidObj[recObj.tid];
                }
                else if (recObj.tid == 10) {////代号10表示一个固定方法，表示已经匹配到对手
                    if (typeof gameMatchedCallback == 'function') {
                        gameMatchedCallback(recObj);
                    }
                    else {
                        alert('gameMatchedCallback not a function');
                    }
                } else if (recObj.tid == 11) {////代号11表示一个固定方法，表示会员加入，离开有异动的时候，通知相关人员
                    if (typeof RoomMatchedCallback == 'function') {
                        RoomMatchedCallback(recObj);
                    }
                    else {
                        alert('gameMatchedCallback not a function');
                    }
                }
            }

            wsObj.onclose = function () {
                wsObj = null;
                state = readyState[1];
                //alert(state);
                top.window.location.href = 'index.html?token=042CABF8-A62C-48D6-8E09-F8B3FE8BBA20';
            }
        }
        catch (e) {
            alert(e);
            if (wsObj != null) {
                wsObj.close();
            }
            setTimeout(this.connect, 500000);
        }
    };

    this.send = function (action, data, callback) {
        if (wsObj == null) return null;

        var tid = new Date().getTime().toString();

        tidObj[tid] = callback;

        data.tid = tid;

        wsObj.send(action + ' ' + $.toJSON(data));
    };

    this.getState = function () {
        return state;
    }
}

var wscore = new wsObject();
wscore.connect(); 