(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[2373],{93045:function(a,b,c){(window.__NEXT_P=window.__NEXT_P||[]).push(["/user/create-deposit",function(){return c(72775)}])},72775:function(a,b,c){"use strict";c.r(b);var d=c(85893),e=c(55241),f=c(27049),g=c(11163),h=c(67294),i=c(87536),j=c(88767),k=c(25617),l=c(48880),m=c(61474),n=c(93702),o=c(9861),p=c(90198),q=function(){var a,b,c,n=(0,k.v9)(function(a){return a.userCurretnInfo}),q=(0,h.useState)(!1),r=q[0],s=q[1],t=(0,j.useQueryClient)(),u=(0,p.N)({warehouseTQEnabled:!0,warehouseVNEnabled:!0,shippingTypeToWarehouseEnabled:!0}),v=u.warehouseTQ,w=u.warehouseVN,x=u.shippingTypeToWarehouse,y=(0,i.cI)({mode:"onBlur",defaultValues:{smallPackages:[{Amount:1,Category:null,IsCheckProduct:!1,IsPacked:!1,IsInsurance:!1,FeeShip:0},],ShippingTypeId:null===(a=null==x?void 0:x.find(function(a){return a.Id===(null==n?void 0:n.ShippingType)}))|| void 0===a?void 0:a.Id,WareHouseFromId:null===(b=null==v?void 0:v.find(function(a){return a.Id===(null==n?void 0:n.WarehouseFrom)}))|| void 0===b?void 0:b.Id,WareHouseId:null===(c=null==w?void 0:w.find(function(a){return a.Id===(null==n?void 0:n.WarehouseTo)}))|| void 0===c?void 0:c.Id}}),z=y.control,A=y.reset,B=y.handleSubmit,C=y.setValue,D=(0,i.Dq)({control:z,name:"smallPackages",keyName:"Id"}),E=D.fields,F=D.append,G=D.remove;(0,o.Hu)(function(){var a,b,c;A({smallPackages:[{Amount:1,Category:null,IsCheckProduct:!1,IsPacked:!1,IsInsurance:!1,FeeShip:0},],ShippingTypeId:null===(a=null==x?void 0:x.find(function(a){return a.Id===(null==n?void 0:n.ShippingType)}))|| void 0===a?void 0:a.Id,WareHouseFromId:null===(b=null==v?void 0:v.find(function(a){return a.Id===(null==n?void 0:n.WarehouseFrom)}))|| void 0===b?void 0:b.Id,WareHouseId:null===(c=null==w?void 0:w.find(function(a){return a.Id===(null==n?void 0:n.WarehouseTo)}))|| void 0===c?void 0:c.Id})},[v,w,x]);var H=(0,j.useMutation)(function(a){return l.V8.create(a)},{onSuccess:function(){var a,b,c;m.Amu.success("Tạo đơn h\xe0ng k\xfd gửi th\xe0nh c\xf4ng"),A({UID:n.Id,smallPackages:[{Amount:1,Category:null,IsCheckProduct:!1,IsPacked:!1,IsInsurance:!1,FeeShip:0},],ShippingTypeId:null===(a=null==x?void 0:x.find(function(a){return a.Id===(null==n?void 0:n.ShippingType)}))|| void 0===a?void 0:a.Id,WareHouseFromId:null===(b=null==v?void 0:v.find(function(a){return a.Id===(null==n?void 0:n.WarehouseFrom)}))|| void 0===b?void 0:b.Id,WareHouseId:null===(c=null==w?void 0:w.find(function(a){return a.Id===(null==n?void 0:n.WarehouseTo)}))|| void 0===c?void 0:c.Id}),t.invalidateQueries({queryKey:"menuData"}),g.default.push("/user/deposit-list")},onError:function(a){m.Amu.error(a),s(!1)}}),I=function(a){s(!0);var b=!0;a.smallPackages.forEach(function(a){if(!a.Category||!a.Amount){m.Amu.warning("Loại sản phẩm hoặc số lượng đang để trống!"),b=!1,s(!1);return}}),b&&(delete a.UID,H.mutateAsync(a))};return(0,d.jsxs)(h.Fragment,{children:[(0,d.jsxs)("div",{className:"flex w-fit ml-auto",children:[(0,d.jsx)(m.Kkx,{onClick:function(){return F({Amount:null,OrderTransactionCode:null,Category:null,IsCheckProduct:!1,IsPacked:!1,IsInsurance:!1,Kg:0,UserNote:null,FeeShip:null})},title:"Th\xeam kiện",icon:"far fa-plus",isButton:!0,isButtonClassName:"bg-green !text-white mr-2"}),(0,d.jsx)(e.Z,{trigger:"click",placement:"bottomLeft",content:(0,d.jsxs)("div",{className:"grid grid-cols-4 p-4 w-[500px]",children:[(0,d.jsx)("div",{className:"col-span-4 grid grid-col-2",children:(0,d.jsx)(m.CBz,{control:z,warehouseTQCatalogue:v,warehouseVNCatalogue:w,shippingTypeToWarehouseCatalogue:x,append:F,user:n})}),(0,d.jsx)("div",{className:"col-span-4",children:(0,d.jsx)(f.Z,{className:"!my-4"})}),(0,d.jsx)("div",{className:"col-span-4 flex items-end justify-end",children:(0,d.jsx)(m.Kkx,{onClick:B(I),icon:"fas fa-check-circle",disabled:r,title:"Tạo đơn",isButton:!0,isButtonClassName:"bg-main !text-white"})})]}),children:(0,d.jsx)(m.Kkx,{icon:"mr-0",title:"Tiếp tục",isButton:!0,isButtonClassName:"bg-sec !text-white"})})]}),(0,d.jsx)(m.PYV,{data:E,control:z,handleSubmit:B,onPress:I,remove:G,setValue:C})]})};q.displayName=n.W.consignmentShipping.createOderDeposit,q.breadcrumb="Tạo đơn k\xfd gửi",q.Layout=m.rfd,b.default=q}},function(a){a.O(0,[675,296,3662,7570,7281,9930,9774,2888,179],function(){return a(a.s=93045)}),_N_E=a.O()}])