(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[9863],{37647:function(a,b,c){(window.__NEXT_P=window.__NEXT_P||[]).push(["/manager/statistical/sales",function(){return c(21406)}])},21406:function(a,b,c){"use strict";c.r(b);var d=c(28520),e=c.n(d),f=c(85893),g=c(11163),h=c(67294),i=c(88767),j=c(25617),k=c(48880),l=c(61474),m=c(12023),n=c(93702);function o(a,b,c,d,e,f,g){try{var h=a[f](g),i=h.value}catch(j){c(j);return}h.done?b(i):Promise.resolve(i).then(d,e)}function p(a,b,c){return b in a?Object.defineProperty(a,b,{value:c,enumerable:!0,configurable:!0,writable:!0}):a[b]=c,a}var q=function(){var a=(0,j.v9)(function(a){return a.userCurretnInfo}),b=(0,h.useState)("detail"),c=b[0],d=b[1],n=(0,h.useState)(null),q=n[0],r=n[1],s=(0,h.useState)(null),t=s[0],u=s[1],v=(0,h.useState)(m.G2),w=v[0],x=v[1],y=(0,i.useQuery)(["clientOrderReportData",{Current:w.current,FromDate:q,ToDate:t,UID:null==a?void 0:a.Id,RoleID:null==a?void 0:a.UserGroupId},],function(){return k._b.getList({PageIndex:w.current,PageSize:w.pageSize,OrderBy:"Id desc",FromDate:q,ToDate:t,UID:null==a?void 0:a.Id,RoleID:null==a?void 0:a.UserGroupId,Status:2}).then(function(a){return a.Data})},{onSuccess:function(a){x(function(a){for(var b=1;b<arguments.length;b++){var c=null!=arguments[b]?arguments[b]:{},d=Object.keys(c);"function"==typeof Object.getOwnPropertySymbols&&(d=d.concat(Object.getOwnPropertySymbols(c).filter(function(a){return Object.getOwnPropertyDescriptor(c,a).enumerable}))),d.forEach(function(b){p(a,b,c[b])})}return a}({},w,{total:null==a?void 0:a.TotalItem}))},onError:function(a){var b,c,d;return l.Amu.error(null===(b=a)|| void 0===b?void 0:null===(c=b.response)|| void 0===c?void 0:null===(d=c.data)|| void 0===d?void 0:d.ResultMessage)}}),z=y.data,A=y.isFetching;y.isLoading;var B=(0,i.useQuery)(["get-total-overview",{fromDate:q,toDate:t,UID:null==a?void 0:a.Id,RoleID:null==a?void 0:a.UserGroupId},],function(){return k._b.getTotalOverview({FromDate:q,ToDate:t,UID:null==a?void 0:a.Id,RoleID:null==a?void 0:a.UserGroupId})},{onError:function(a){var b,c,d;l.Amu.error(null===(b=a)|| void 0===b?void 0:null===(c=b.response)|| void 0===c?void 0:null===(d=c.data)|| void 0===d?void 0:d.ResultMessage)}}),C=B.data,D=function(a){return function(){var b=this,c=arguments;return new Promise(function(d,e){var f=a.apply(b,c);function g(a){o(f,d,e,g,h,"next",a)}function h(a){o(f,d,e,g,h,"throw",a)}g(void 0)})}}(e().mark(function b(){var c;return e().wrap(function(b){for(;;)switch(b.prev=b.next){case 0:return b.prev=0,b.next=3,k._b.export({UID:null==a?void 0:a.Id,RoleID:null==a?void 0:a.UserGroupId});case 3:c=b.sent,g.default.push("".concat(c.Data)),b.next=10;break;case 7:b.prev=7,b.t0=b.catch(0),l.Amu.error(b.t0);case 10:case"end":return b.stop()}},b,null,[[0,7]])}));return(0,f.jsxs)("div",{className:"grid grid-cols-12 gap-4",children:[(0,f.jsxs)("div",{className:"tableBox col-span-8 h-fit",children:[(0,f.jsx)(l.up4,{handleFilter:function(a,b){r(a),u(b)},type:c,handleType:function(){return d(function(a){return"detail"===a?"sum":"detail"})},resetPagination:function(){x(m.G2)}}),(0,f.jsx)(l.rwE,{type:c,dataChart:null==C?void 0:C.Data})]}),(0,f.jsx)("div",{className:"col-span-4",children:(0,f.jsx)(l.n3Q,{data:null==C?void 0:C.Data})}),(0,f.jsx)("div",{className:"col-span-12",children:(0,f.jsx)(l.CD$,{pagination:w,handlePagination:x,loading:A,data:null==z?void 0:z.Items,exportExcel:D,RoleID:null==a?void 0:a.UserGroupId})})]})};q.displayName=n.B.statistical.turnover,q.breadcrumb=m.m.statistical.sales,q.Layout=l.Ar2,b.default=q}},function(a){a.O(0,[675,296,3662,7570,7281,9930,9774,2888,179],function(){return a(a.s=37647)}),_N_E=a.O()}])