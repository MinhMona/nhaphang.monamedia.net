(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[6310],{19513:function(a,b,c){(window.__NEXT_P=window.__NEXT_P||[]).push(["/manager/money/recharge-history",function(){return c(49734)}])},49734:function(a,b,c){"use strict";c.r(b);var d=c(85893),e=c(11163),f=c(67294),g=c(88767),h=c(72132),i=c(48880),j=c(61474),k=c(12023),l=c(93702),m=c(12954);function n(a,b,c){return b in a?Object.defineProperty(a,b,{value:c,enumerable:!0,configurable:!0,writable:!0}):a[b]=c,a}function o(a){for(var b=1;b<arguments.length;b++){var c=null!=arguments[b]?arguments[b]:{},d=Object.keys(c);"function"==typeof Object.getOwnPropertySymbols&&(d=d.concat(Object.getOwnPropertySymbols(c).filter(function(a){return Object.getOwnPropertyDescriptor(c,a).enumerable}))),d.forEach(function(b){n(a,b,c[b])})}return a}var p="col-span-1 tableBox cardTopTable p-2 items-center",q="tableBox cardTopTable col-span-1 w-full p-3",r=function(){var a,b,c,k,l,n,r,s,t,u,v,w,x,y,z,A=(0,f.useState)({PageIndex:1,PageSize:20,TotalItems:null,SearchContent:null,Status:null,FromDate:null,ToDate:null}),B=A[0],C=A[1],D=function(a){C(o({},B,a))},E=(0,f.useRef)(),F=(0,f.useState)(!1),G=F[0],H=F[1],I=(0,g.useQuery)(["clientRechargeData",o({},B)],function(){return i.vy.getList(B).then(function(a){return a.Data})},{onSuccess:function(a){return C(o({},B,{TotalItems:null==a?void 0:a.TotalItem,PageIndex:null==a?void 0:a.PageIndex,PageSize:null==a?void 0:a.PageSize})),null==a?void 0:a.Items},onError:function(a){var b,c,d;h.Am.error(null===(b=a)|| void 0===b?void 0:null===(c=b.response)|| void 0===c?void 0:null===(d=c.data)|| void 0===d?void 0:d.ResultMessage)}}),J=I.data,K=I.isFetching,L=I.isError;return L?(0,d.jsx)(j.TXS,{}):(0,d.jsxs)(d.Fragment,{children:[(0,d.jsxs)("div",{className:"grid grid-cols-4 gap-2 mb-4",children:[(0,d.jsxs)("div",{className:p,children:["Tổng đơn",(0,d.jsx)("span",{className:"text-bold text-blue font-semibold text-[20px]",children:null==J?void 0:J.TotalItem})]}),(0,d.jsxs)("div",{className:p,children:["Số đơn đ\xe3 duyệt",(0,d.jsx)("span",{className:"text-bold text-green font-semibold text-[20px]",children:null!==(x=null==J?void 0:null===(a=J.Items)|| void 0===a?void 0:null===(b=a[0])|| void 0===b?void 0:b.TotalStatus2)&& void 0!==x?x:0})]}),(0,d.jsxs)("div",{className:p,children:["Số đơn chờ duyệt",(0,d.jsx)("span",{className:"text-bold text-[#f7b467] font-semibold text-[20px]",children:null!==(y=null==J?void 0:null===(c=J.Items)|| void 0===c?void 0:null===(k=c[0])|| void 0===k?void 0:k.TotalStatus1)&& void 0!==y?y:0})]}),(0,d.jsxs)("div",{className:p,children:["Số đơn đ\xe3 huỷ",(0,d.jsx)("span",{className:"text-bold text-red font-semibold text-[20px]",children:null!==(z=null==J?void 0:null===(l=J.Items)|| void 0===l?void 0:null===(n=l[0])|| void 0===n?void 0:n.TotalStatus3)&& void 0!==z?z:0})]})]}),(0,d.jsxs)("div",{className:"grid grid-cols-3 gap-2 mb-4",children:[(0,d.jsxs)("div",{className:q,children:[(0,d.jsx)("div",{className:"IconBoxFilter IconFilter text-white bg-[#e75b5b]",children:(0,d.jsx)("i",{className:"fas fa-sack-dollar"})}),(0,d.jsxs)("div",{children:[(0,d.jsx)("div",{className:"text-right",children:"Tổng số tiền:"}),(0,d.jsx)("span",{className:"font-bold text-base text-[#e75b5b] flex items-center justify-end",children:m.Jy.getVND(null==J?void 0:null===(r=J.Items)|| void 0===r?void 0:null===(s=r[0])|| void 0===s?void 0:s.TotalAmount)})]})]}),(0,d.jsxs)("div",{className:q,children:[(0,d.jsx)("div",{className:"IconBoxFilter text-white bg-green IconFilter",children:(0,d.jsx)("i",{className:"fas fa-sack-dollar"})}),(0,d.jsxs)("div",{children:[(0,d.jsx)("div",{className:"text-right",children:"Tổng số tiền đ\xe3 duyệt:"}),(0,d.jsx)("span",{className:"font-bold text-base text-green flex items-center justify-end",children:m.Jy.getVND(null==J?void 0:null===(t=J.Items)|| void 0===t?void 0:null===(u=t[0])|| void 0===u?void 0:u.TotalAmount2)})]})]}),(0,d.jsxs)("div",{className:q,children:[(0,d.jsx)("div",{className:"IconBoxFilter text-white bg-main IconFilter",children:(0,d.jsx)("i",{className:"fas fa-sack-dollar"})}),(0,d.jsxs)("div",{children:[(0,d.jsx)("div",{className:"text-right",children:"Tổng số tiền chờ duyệt:"}),(0,d.jsx)("span",{className:"font-bold text-base text-main flex items-center justify-end",children:m.Jy.getVND(null==J?void 0:null===(v=J.Items)|| void 0===v?void 0:null===(w=v[0])|| void 0===w?void 0:w.TotalAmount1)})]})]})]}),(0,d.jsxs)("div",{className:"",children:[(0,d.jsx)(j.k39,{handleFilter:D,handleExportExcel:function(){i.vy.exportExcel(o({},B,{PageSize:99999})).then(function(a){e.default.push(a.Data)}).catch(function(a){var b,c,d;h.Am.error(null===(b=a)|| void 0===b?void 0:null===(c=b.response)|| void 0===c?void 0:null===(d=c.data)|| void 0===d?void 0:d.ResultMessage)})}}),(0,d.jsx)(j.J6g,{data:null==J?void 0:J.Items,filter:B,handleModal:function(a){E.current=a,H(!0)},loading:K,handleFilter:D}),(0,d.jsx)(j.XWT,{visible:G,onCancel:function(){return H(!1)},defaultValues:E.current})]})]})};r.displayName=l.B.moneyManagement.historyRechargeVN,r.breadcrumb=k.m.money.rechargeHistory,r.Layout=j.Ar2,b.default=r}},function(a){a.O(0,[675,296,3662,7570,7281,9930,9774,2888,179],function(){return a(a.s=19513)}),_N_E=a.O()}])