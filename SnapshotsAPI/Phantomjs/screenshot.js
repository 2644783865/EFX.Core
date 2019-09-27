console.log('Hello, world!');
var page = require('webpage').create(),
    system = require('system'),
    address, output, size;
if (system.args.length < 3 || system.args.length > 5) {
    console.log('Usage: rasterize.js URL filename');
    phantom.exit(1);
} else {
    address = system.args[1];
    output = system.args[2];
    page.viewportSize = { width: 1200, height: 600 };
    page.open(address, function (status) {
      // ͨ����ҳ����ִ�нű���ȡҳ�����Ⱦ�߶�
      var bb = page.evaluate(function () { 
        return document.getElementsByTagName('html')[0].getBoundingClientRect(); 
      });
	  
      // ����ʵ��ҳ��ĸ߶ȣ��趨��Ⱦ�Ŀ��
      page.clipRect = {
        top:    bb.top,
        left:   bb.left,
        width:  bb.width,
        height: bb.height
      };
      // Ԥ��һ������Ⱦʱ��
      window.setTimeout(function () {
        page.render(output);
        page.close();
        console.log('render ok');
		phantom.exit();
      }, 1000);
    });
}
//phantom.exit();  #������ֹphtomjs����