(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[4449],{65228:function(a,b,c){(window.__NEXT_P=window.__NEXT_P||[]).push(["/manager/notification",function(){return c(13409)}])},12282:function(a,b,c){"use strict";c.d(b,{Z:function(){return v}});var d=c(85893),e=c(28985),f=c(41664),g=c(67294),h=c(72132),i=c(48880),j=c(12741),k=c(64162),l=c(12954),m=c(28004),n=c(28520),o=c.n(n),p=c(30381),q=c.n(p),r=c(61474);function s(a,b,c,d,e,f,g){try{var h=a[f](g),i=h.value}catch(j){c(j);return}h.done?b(i):Promise.resolve(i).then(d,e)}var t=function(a){var b=a.handleFilter,c=a.isFetching,e=(0,g.useRef)(null),f=(0,g.useRef)(null);function h(){return i.apply(this,arguments)}function i(){return(i=(function(a){return function(){var b=this,c=arguments;return new Promise(function(d,e){var f=a.apply(b,c);function g(a){s(f,d,e,g,h,"next",a)}function h(a){s(f,d,e,g,h,"throw",a)}g(void 0)})}})(o().mark(function a(c){var d,g;return o().wrap(function(a){for(;;)switch(a.prev=a.next){case 0:if(d=void 0!==c&&c,b){a.next=3;break}return a.abrupt("return");case 3:if(d){a.next=6;break}return b({FromDate:e.current,ToDate:f.current}),a.abrupt("return");case 6:b({FromDate:g=q()().format("YYYY-MM-DD"),ToDate:g});case 8:case"end":return a.stop()}},a)}))).apply(this,arguments)}return(0,d.jsxs)("div",{className:"xl:flex w-fit items-end",children:[(0,d.jsx)("div",{className:"mr-4 mb-4 xl:mb-0 w-[400px]",children:(0,d.jsx)(r.jwb,{disabled:c,format:"DD/MM/YYYY",placeholder:"Từ ng\xe0y / đến ng\xe0y",handleDate:function(a){e.current=a[0],f.current=a[1]}})}),(0,d.jsx)("div",{children:(0,d.jsx)(r.hU,{icon:"fas fa-search",title:"T\xecm?",onClick:h,showLoading:!0,toolip:"",disabled:c})}),(0,d.jsx)("div",{className:"ml-4",children:(0,d.jsx)(r.hU,{icon:"far fa-info-square",title:"Lọc h\xf4m nay",onClick:function(){h(!0)},showLoading:!0,toolip:"",disabled:c})})]})};function u(a,b,c){return b in a?Object.defineProperty(a,b,{value:c,enumerable:!0,configurable:!0,writable:!0}):a[b]=c,a}var v=function(a){var b=a.data,c=a.loading,n=a.handleFilter,o=a.filter,p=a.refetch,q=a.isFetching,r=(0,g.useState)([]),s=r[0],v=r[1];return(0,d.jsxs)(d.Fragment,{children:[(0,d.jsx)(k.w,{loading:c,columns:[{dataIndex:"Id",title:"STT",width:30,render:function(a,b,c){return++c}},{dataIndex:"NotificationContent",title:"Nội dung",responsive:["lg"],width:200},{dataIndex:"TotalPriceReceive",title:"Trạng th\xe1i",responsive:["xl"],width:120,render:function(a,b){return(0,d.jsx)(m.Z,{color:b.IsRead?"blue":"red",statusName:b.IsRead?"Đ\xe3 xem":"Chưa xem"})}},{dataIndex:"Created",title:"Ng\xe0y",width:130,render:function(a){return l.Jy.getVNDate(a)},responsive:["sm"]},{dataIndex:"GoToDetail",title:"Xem chi tiết",align:"center",width:90,render:function(a,b){return(0,d.jsx)(f.default,{href:null==b?void 0:b.Url,children:(0,d.jsx)("a",{style:{opacity:b.Url?"1":"0.3",pointerEvents:b.Url?"all":"none",cursor:b.Url?"pointer":"none"},target:"_blank",children:(0,d.jsx)(j.K,{icon:"far fa-info-square",title:"Xem chi tiết",isButton:!0,onClick:function(){b.IsRead||(b.IsRead=!0,i.Cu.readNotify([null==b?void 0:b.Id]))}})})})}},],data:b,bordered:!0,expandable:{expandedRowRender:function(a){return(0,d.jsxs)("ul",{className:"px-2 text-xs",children:[(0,d.jsxs)("li",{className:"sm:hidden justify-between flex py-2",children:[(0,d.jsx)("span",{className:"font-medium mr-4",children:"Ng\xe0y:"}),(0,d.jsx)("div",{children:l.Jy.getShortVNDate(null==a?void 0:a.Created)})]}),(0,d.jsxs)("li",{className:"md:hidden justify-between flex py-2",children:[(0,d.jsx)("span",{className:"font-medium mr-4",children:"Nội dung:"}),(0,d.jsx)("div",{children:null==a?void 0:a.NotificationContent})]}),(0,d.jsxs)("li",{className:"xl:hidden justify-between flex py-2",children:[(0,d.jsx)("span",{className:"font-medium mr-4",children:"Trạng th\xe1i:"}),(0,d.jsx)("div",{children:null==a?void 0:a.TotalPriceReceive})]})]})}},scroll:{y:700},rowSelection:{selectedRowKeys:s,getCheckboxProps:function(a){return a.IsRead?{name:a.Id.toString(),disabled:!0,className:"!hidden"}:{name:a.Id.toString(),disabled:!1}},onChange:function(a,b){v(a)}},extraElmentClassName:"flex !w-full items-end justify-between",extraElment:(0,d.jsxs)(d.Fragment,{children:[(0,d.jsx)(t,{handleFilter:n,isFetching:q}),s.length>0&&(0,d.jsx)(j.K,{title:"Đ\xe1nh dấu đ\xe3 đọc",icon:"!mr-0",isButton:!0,isButtonClassName:"h-fit bg-blue !text-white",onClick:function(){var a=h.Am.loading("Đang xử l\xfd ...");i.Cu.readNotify(s).then(function(){h.Am.update(a,{render:"Đ\xe3 đọc th\xf4ng b\xe1o!",isLoading:!1,autoClose:1000,type:"success"}),v([]),p()}).catch(function(b){var c,d,e;h.Am.update(a,{render:null===(c=b)|| void 0===c?void 0:null===(d=c.response)|| void 0===d?void 0:null===(e=d.data)|| void 0===e?void 0:e.ResultMessage,isLoading:!1,autoClose:1000,type:"error"})})}})]})}),(0,d.jsx)("div",{className:"mt-4 text-right",children:(0,d.jsx)(e.Z,{total:null==o?void 0:o.TotalItems,current:null==o?void 0:o.PageIndex,pageSize:null==o?void 0:o.PageSize,onChange:function(a,b){return n(function(a){for(var b=1;b<arguments.length;b++){var c=null!=arguments[b]?arguments[b]:{},d=Object.keys(c);"function"==typeof Object.getOwnPropertySymbols&&(d=d.concat(Object.getOwnPropertySymbols(c).filter(function(a){return Object.getOwnPropertyDescriptor(c,a).enumerable}))),d.forEach(function(b){u(a,b,c[b])})}return a}({},o,{PageIndex:a,PageSize:b}))}})})]})}},13409:function(a,b,c){"use strict";c.r(b);var d=c(85893),e=c(67294),f=c(88767),g=c(25617),h=c(48880),i=c(61474),j=c(12282),k=c(12023),l=c(93702);function m(a,b,c){return b in a?Object.defineProperty(a,b,{value:c,enumerable:!0,configurable:!0,writable:!0}):a[b]=c,a}function n(a){for(var b=1;b<arguments.length;b++){var c=null!=arguments[b]?arguments[b]:{},d=Object.keys(c);"function"==typeof Object.getOwnPropertySymbols&&(d=d.concat(Object.getOwnPropertySymbols(c).filter(function(a){return Object.getOwnPropertyDescriptor(c,a).enumerable}))),d.forEach(function(b){m(a,b,c[b])})}return a}var o=function(a){var b=a.connection,c=(0,g.v9)(function(a){return a.userCurretnInfo}),k=(0,e.useState)({TotalItems:null,PageIndex:1,PageSize:20,FromDate:null,ToDate:null,OrderBy:"Id desc",UID:null==c?void 0:c.Id,OfEmployee:!0,IsRead:0}),l=k[0],m=k[1],o=(0,f.useQuery)(["menuData",[l.PageIndex,l.ToDate,l.FromDate,l.UID],],function(){return h.t6.getList(l).then(function(a){var b,c,d;return m(n({},l,{TotalItems:null==a?void 0:null===(b=a.Data)|| void 0===b?void 0:b.TotalItem,PageSize:null==a?void 0:null===(c=a.Data)|| void 0===c?void 0:c.PageSize})),(null==q?void 0:null===(d=q.Items)|| void 0===d?void 0:d.length)<=0&&i.Amu.info("Kh\xf4ng c\xf3 th\xf4ng b\xe1o trong khoảng thời gian n\xe0y!"),null==a?void 0:a.Data})},{retry:!1,enabled:!!(null==c?void 0:c.Id),keepPreviousData:!0,staleTime:10e3,onError:i.Amu.error}),p=o.isFetching,q=o.data,r=o.refetch;return(0,e.useEffect)(function(){b&&b.on("send-notification",function(a){return null==q?void 0:q.Items.unshift(a)})},[b]),(0,d.jsx)(j.Z,{isFetching:p,refetch:r,data:null==q?void 0:q.Items,loading:p,handleFilter:function(a){m(n({},l,a))},filter:l})};o.displayName=l.B.notification,o.breadcrumb=k.m.notification,o.Layout=i.Ar2,b.default=o}},function(a){a.O(0,[675,296,3662,7570,7281,9930,9774,2888,179],function(){return a(a.s=65228)}),_N_E=a.O()}])