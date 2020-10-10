var menuRole;
var menuName;
var menuURL;
var menuFileName;

$(document).ready(function(){
    $("#LoginMenu").on('shown.bs.modal', function(){
        $(this).find('#user-id').focus();
    });

    $("#ModalMachine").on("hidden.bs.modal", function () {
        gUserSup = "";
    });
    
    $('[data-toggle="tooltip"]').tooltip();   
});

var gResetId = "";
var gUserSup = "";
var gSupComfirmText = "SUPERVISOR CONFIRM";
function GetTitle(getmenuName,getuserRole,getmenuURL,getmenuFileName)
{
    $("#result").empty();
    $("#user-id").val(""); 
    $("#title") .html(getmenuName);
    menuName = getmenuName;
    menuRole = getuserRole;
    menuURL = getmenuURL;
    menuFileName = getmenuFileName;

    if (getmenuName == gSupComfirmText.toString()) //new
    {
        gResetId = gSupComfirmText.toString();
    }
}

function ShowAlert()
{
    $('#myAlert').show('fade');

    setTimeout(function(){
        $('#myAlert').hide('fade');
    }, 2000);

    $('#user-id').focus();
}

function LogIn()
{   
    if(document.getElementById('user-id').value=="")
    {
        //$('#txtMessage').html('<strong>Error!</strong> Hãy nhập mã số nhân viên');
        //showAlert();
        ShowToast(1, "Hãy nhập mã số nhân viên");
        return;
    }
    gUserSup = document.getElementById('user-id').value;
    //20200907 check special character in barcode supervisor
    if (gResetId != "")
    {
        if (gUserSup.substring(0, 1) !== "¤")
        {
            ShowToast(1, "Vui lòng quét barcode Supervisor!!!");
            return;
        }
    }
    //end check special character in barcode supervisor
    $.ajax({    
        url:'/Home/CheckUser', 
        data: {
            MenuRole: menuRole,          
            UserId : document.getElementById('user-id').value.substring(1),
        },
        cache:'false',
        dataType:'json',        
        type:'POST',      
        success: function(results) { 
            if(results == "OK")
            {   
                if (gResetId != gSupComfirmText.toString())     
                {
                    window.location.assign("/" + menuURL + "/" + menuFileName); 
                }
                else 
                {
                    //hide login
                    $("#LoginMenu").modal('hide');
                    //show popup machine
                    $("#ModalMachine").modal('show');
                    title.innerText = "Supervisor confirm";
                    
                    LoadMachineToModal();
                }
            }   
            else
            {    
                // $('#txtMessage').html('<strong>Error!</strong> Bạn không có quyền truy cập');
                // showAlert();
                ShowToast(1, "Bạn không có quyền truy cập");
            }                             
        }
    })     
}

function runButton(e) {   
    if (e.keyCode == 13) {
        document.getElementById("btnCloseModal").click();
        return false;
    }
}

//start 20200520
//fetch line
//open modal line
$(window).on('load',function() { //when load page, show popup immediately
   
    var checkLine = JSON.parse(window.sessionStorage.getItem('line_no'));
    if (checkLine === null)
    {
        $('#ModalLineNo').modal('show');
        FetchSection();
        return;
    }
    idLine.innerText = checkLine.line_no;
    gLineNo = checkLine.line_no;
});

var sectionList = document.getElementById("SectionID");
function FetchSection() { //its has another way to solve this situation.
    $.ajax({
        url: "/Home/GetSection",
        type: "GET",
        contentType: "application/json, charset=utf-8",
        dataType: "json",
        data: {},
        success: function(res) {

            if (res.success === false)
            {
                ShowToast(1, res.error);
                return;
            }
            if (res.section.length <= 0)
            {
                ShowToast(1, res.error);
                return;
            }
            var section = JSON.parse(res.section);
            //draw here
            var html = '';
            for (let i = 0; i < section.length; i++) {
                var id = section[i].section_id.trim();
                html += '<li><a id="'+ id +'" class="dropdown-item" onClick="SectionClick(this.id)" style="border-radius: 5px;" >' + id + '</a></li>'
                if (i != section.length)
                {
                    html += '<li role="separator" class="divider" style="border-radius: 5px;" ></li>'    
                }
            }
            sectionList.innerHTML = html;
            $('#SectionID li').on("click", SectionChooses)  //raise event
            
        },
        error: function(mes) {
            //adjust later
            //console.log(mes);
            ShowToast(2, "FetchSection() err...")
        }
    });
}
//chossection here
function SectionChooses() {
    $('#TopSection').html($(this).text() + '<span class="caret"></span>');
}
//when click on it
function SectionClick(sectionId) {
    FecthLineBySection(sectionId);
}
function ResetMachine() {
    $('#ModalLineNo').modal('show');
    FetchSection();
    return;
}
//pushhhhhhhhh 
var gLineNo;
const localLine = {}
var idLine = document.getElementById("LnID");

function PushLine() {
    var lineId = document.getElementById("TopLine");
    
    if (lineId.textContent.trim() === "Choose line" || lineId === undefined)
    {
        //alert("Bạn chưa chọn line");
        ShowToast(1, "Bạn chưa chọn line");
        return;
    }
    idLine.innerText = lineId.textContent.trim(); //ok now get line

    gLineNo = lineId.textContent.trim(); //set global varible.
    localLine.line_no = gLineNo;
    window.sessionStorage.setItem("line_no", JSON.stringify(localLine));
    $("#btnSaveLine").attr("data-dismiss", "modal");
}

var linenoList = document.getElementById("LineID");

function FecthLineBySection(sec) {
    $.ajax({
        url: "/Home/GetLineBySection",
        type: "GET",
        contentType: "application/json, charset=utf-8",
        dataType: "json",
        data: { section: sec },
        success: function(res) {
            if (res.success == false)
            {
                ShowToast(1, res.error);
                return;
            }
            if (res.sectionLine.length <= 0)
            {
                ShowToast(1, res.error);
                return;
            }
            var sectionLine = JSON.parse(res.sectionLine);

            var html = '';
            for (let i = 0; i < sectionLine.length; i++) {
                var line = sectionLine[i].line_no.trim();
                html += '<li><a id="'+ line +'" class="dropdown-item" style="border-radius: 5px;" >' + line + '</a></li>'
            }
            linenoList.innerHTML = html;
            $('#LineID li').on("click", LineClick)  //raise event
        },
        error: function(mes) {
            //console.log(mes);
            ShowToast(2, "FecthLineBySection() err...")
        }
    });
}
function LineClick(){
    $('#TopLine').html($(this).text() + '<span class="caret"></span>');
}
//end
var colorType = {
    selected: "#32CD32",
    deselected: "DeepSkyBlue",
    warning: "#D2691E",
}

var title = document.getElementById("CardTitle"); //set title name
var idValue;
var cardContent = document.getElementById("cardContent");
var StatusSelectedMachineArr = [];
var gUser;
var typeSelected;
//when card click
function MenuClick(id, name) {
    idValue = id;
    $('#ModalMachine').on('show.bs.modal', function (e) {
        title.innerText = name;
    });
    //fetch data here
    RefreshArr();
    LoadMachineToModal();
    if (gResetId === "")
    {   
        showDivRst.style.display = "none";
    }
}

var errorListMachine = [];

function LoadMachineToModal()
{
    var checkLine = JSON.parse(window.sessionStorage.getItem('line_no'));
    if (checkLine === null)
    {
        $('#ModalLineNo').modal('show');
        FetchSection();
        return;
    }
    gLineNo = checkLine.line_no;
    cardContent.innerHTML = '<div>Loading...</div>'
    $.ajax({
        url: '/Home/GetMachineByLine',
        type: "GET",
        contentType : "application/json; charset=utf-8",
        dataType: "json",
        data: {
            lineNo: gLineNo,
        },
        // fetch machine
        success: function (result) { 

            if (result.success == false) {
                //adjust later
                //alert("fetch data error")
                RefreshArr();
                if (result.plc.length > 0) {
                    var dataPlcErrJson = JSON.parse(result.plc);
                    ShowToast(2, "Không gửi được PLC máy " + dataPlcErrJson[0].toString().trim() + ". Vui lòng thao tác lại");
                    ShowToast(2, "ret = " + result.error + "rcv = " + result.dataErr);
                    $('#ModalMachine').modal('hide');
                    return;
                }
                $('#ModalMachine').modal('hide');
                ShowToast(2, "Error = " + result.error);
                return;
            }
            RefreshArr(); //refresh here
            //if canot send to PLC 
            if (result.plc.length > 0)
            {
                var dataPlcErrJson = JSON.parse(result.plc);
                if (dataPlcErrJson.length > 0)
                { 
                    for (let i = 0; i < dataPlcErrJson.length; i++) {
                        ShowToast(2, "Không gửi được PLC máy" + dataPlcErrJson[i].toString().trim() + ". Vui lòng thao tác lại");
                    }  
                }
               
            }
            //end
            //check signal here
            if (result.dataErr.length > 0)
            {
                var dataErrJson = JSON.parse(result.dataErr);
                if (dataErrJson.length > 0)
                { 
                    for (let i = 0; i < dataErrJson.length; i++) {
                        errorListMachine.push(dataErrJson[i]);
                    }    
                }
                errorListMachine = errorListMachine.filter((item, pos) => errorListMachine.indexOf(item) === pos)
            }
            //check load machine here            
            var html = ''; 
            cardContent.innerHTML = "";
            //remove toggle here
            if (result.dataMachine.length <= 0)
            {
                ShowToast(1, result.error);
                return;
            }
            if (result.dataUsr.length <= 0)
            {
                ShowToast(1, result.error);
                return;
            }
            var dataMachine = JSON.parse(result.dataMachine);
            var dataUsr = JSON.parse(result.dataUsr);

            $.each(dataMachine, function(key, item) {
                var machineText = "D" + (key > 10 ? key : "0" + (key + 1 ) ).toString().trim();
                html += '<div id="'+ machineText +'" class="machineType" type="button" >'
                html += '<span id="Usr_'+ machineText +'" class="machineHeader" >...</span>'
                html += '<span>'
                html += key + 1
                html += '</span>'
                html += '<span id="Mes_'+ machineText +'" class="machineChild">...</span>'
                html += '</div>'
            });
           
            cardContent.innerHTML = html;
            if (dataUsr == 0) //khong co may nao dang chay
            {
                $.each(dataMachine, function(key, item) {
                    var machineText = "D" + (key > 10 ? key : "0" + (key + 1 ) ).toString().trim();
                    $("#Usr_"+ machineText ).html(noPlanAndUser.none); 
                    $("#Mes_"+ machineText ).html(noPlanAndUser.noPlan);
                });
            }

            for (let i = 0; i < dataMachine.length; i++) {

                var machineText = dataMachine[i].press_no === null ? "" : dataMachine[i].press_no.trim();
                
                $("#"+ machineText ).removeClass("selected");  //remove toggle here
                
                if (parseInt(dataMachine[i].status) >= 1) //absolutely has curing machine
                {
                    $.each(dataUsr, function(key, item) {

                        if(item.press_no.trim() === dataMachine[i].press_no.trim())
                        {
                            if (item.start_user.trim() !== "" && item.start_user_01.toString().trim() !== "") {
                                gUser = item.start_user_01 === null ? noPlanAndUser.none : item.start_user_01.toString().trim();
                                $("#Usr_"+ machineText ).html(gUser);
                                $("#Mes_"+ machineText ).html(item.menu_name === null ? noPlanAndUser.curing : item.menu_name.trim());
                                $("#"+ machineText).css("background-color", colorType.selected)
                            } else {
                                gUser = item.start_user === null ? noPlanAndUser.none : item.start_user.trim();
                                $("#Usr_"+ machineText ).html(gUser);
                                $("#Mes_"+ machineText ).html(item.menu_name === null ? noPlanAndUser.curing : item.menu_name.trim());
                                $("#"+ machineText).css("background-color", colorType.selected)
                            };
                            
                            typeSelected = item.status.toString().trim();
                            if (item.menu_name !== null)
                            {
                                var chooses = {
                                    pressNo: item.press_no.trim(),
                                    content: item.menu_name === null ? noPlanAndUser.noPlan : item.menu_name.trim(),
                                    status: typeSelected,
                                    user: gUser
                                }
                                StatusSelectedMachineArr.push(chooses);
                            }
    
                            var items = {
                                pressNo: item.press_no.trim(),
                                content: item.menu_name === null ? noPlanAndUser.noPlan : item.menu_name.trim(),
                                status: typeSelected,
                                user: gUser
                            }
                            SelectedPressnoArr.push(items);
                            return false;
                        } 
                        else 
                        {
                            $("#Usr_"+ machineText ).html(noPlanAndUser.none); 
                            $("#Mes_"+ machineText ).html(noPlanAndUser.noPlan);
                        }
                    });
                }
                else 
                {
                    $("#Usr_"+ machineText ).html(noPlanAndUser.none); 
                    $("#Mes_"+ machineText ).html(noPlanAndUser.noPlan);
                }
            }
            SelectedPressnoArr = SelectedPressnoArr.filter((item, pos) => SelectedPressnoArr.indexOf(item) === pos) //remove duplicate
            //show error machine
            if (errorListMachine.length > 0)
            {
                for (let i = 0; i < errorListMachine.length; i++) {
                    $("#"+ errorListMachine[i]).css("background-color", colorType.warning)
                    $("#Mes_"+ errorListMachine[i] ).html(noPlanAndUser.overTime);
                }
            }
             
            $(".machineType").on("click", ClickMachine); //raise event
            
            if (gUserSup !== "") //
            {
                fetchErrorMenu();
                gResetId = ""; //reset modal
            }
            else 
            {
                showDivRst.style.display = "none";
                divAcceptBtn.style.display = "block";
            }
        },
        error: function(errormessage) {
            //console.log(errormessage);
            RefreshArr();
            ShowToast(2, "LoadMachineToModal() err...")
        }
    });
}
//load error information from server, when supervisor wanna reset machine
var menuList = document.getElementById("RstID");
var showDivRst = document.getElementById("divRstSup");
var divAcceptBtn = document.getElementById("divAccept");
var rstMenu = document.getElementById("RstMenu");

function fetchErrorMenu()
{
    $.ajax({
        url: "/Home/GetErrorMenu",
        type: "GET",
        contentType: "application/json, charset-utf-8",
        dataType: "json",
        data: {},
        success: function(res) {
            if (res.success === false)
            {
                ShowToast(1, res.error);
                $("#ModalMachine").modal('hide');
                return;
            }
            if (showDivRst.style.display === "none" || showDivRst.style.display === "") {
                divAcceptBtn.style.display = "none"
                showDivRst.style.display = "inline-flex";
            } 
            rstMenu.textContent = "Chooses reason";

            var menu = JSON.parse(res.menu);
            if (menu.length <= 0)
            {
                ShowToast(1, res.error);
                $("#ModalMachine").modal('hide');
                return;
            }
            //draw here
            var html = '';
            for (let i = 0; i < menu.length; i++) {
                var id = menu[i].error_id.trim();
                var name = menu[i].error_menu.trim();
                html += '<li class="liMenu" style="border-radius: 5px;">'
                html += '<span class="spanMenu">'
                html += (i + 1) + "."
                html += '</span>'
                html += '<a id="'+ id +'" class="dropdown-item" style="padding-left: 10px;" >' + name + '</a>'
                html += '</li>'
            }
            menuList.innerHTML = html;
            $('#RstID li').on("click", menuClick) //raise event
        },
        error: function(mes) {
            RefreshArr();
            ShowToast(2, "fetchErrorMenu() error..")
        }
    });
}

function menuClick(){
    $('#RstMenu').html($(this).text());
}

var machineClickData = []; //array luu cac gia tri phan tu click trung voi cac phan tu load len co du lieu
var itemCheck = []; //array luu lai cac gia tri phan tu da loc
var moreItem = []; //array luu cac machine khi clik them hoac bo chon
var selectedMchineIdsInUse = []; //array chi luu thong tin may'
var selectedMchineIds = []; //array luu cac phan tu khi click
var SelectedPressnoArr = []; //array luu cac phan tu khi co trong td_job_prmold_os
var itemUnselect = [];

function RefreshArr()
{
    machineClickData = [];
    itemCheck = [];
    moreItem = [];
    selectedMchineIdsInUse = [];
    selectedMchineIds = [];
    itemUnselect = [];
    StatusSelectedMachineArr = [];
}

function ClickMachine() { 
    
    $(this).toggleClass('selected'); //this class contains all machine you're clicked
    selectedMchineIds = $('.selected').map(function(index, values, arr) {
        return values;
    }).get();

    selectedMchineIdsInUse = selectedMchineIds;

    SelectedPressnoArr = SelectedPressnoArr.filter((item, pos) => SelectedPressnoArr.indexOf(item) === pos) //loc trung
    if (selectedMchineIdsInUse.length > 0) //array when you click on whatever machine
    { 
        //neu click trung may dang chay
        //chon
        //loc ra nhung machine chon
        if (SelectedPressnoArr.length > 0) {

            selectedMchineIdsInUse = selectedMchineIdsInUse.map((value) => value.id);
            moreItem = SelectedPressnoArr.map((value) => value.pressNo);
            moreItem = moreItem.filter((item, pos) => moreItem.indexOf(item) === pos) //remove duplicate

            machineClickData = moreItem.filter(function(val) {
                return selectedMchineIdsInUse.indexOf(val) != -1;
            });
        }
        machineClickData = machineClickData.filter((item, pos) => machineClickData.indexOf(item) === pos) //loc trung
        //check may' click trung voi may' dang err

        if (title.innerText.trim() === "Supervisor confirm")
        {
            if (errorListMachine.length <= 0)
            {
                $(this).removeClass('selected');
                ShowToast(1, "Bạn chọn không đúng máy cần Reset ClickMachine(1)");
                return;
            }
        }
        if (errorListMachine.length > 0)
        {
            if (gUserSup === "") //neu khong phai supervisor thi click se hien may dang error can phai reset
            {
                //item dang chon co trong list may error
                var erLstFinal = machineClickData.filter(function(n) {
                    return errorListMachine.indexOf(n) !== -1;
                });

                if (erLstFinal.length > 0) //neu 
                {
                    $(this).removeClass('selected');
                    ShowToast(2, "Máy bạn chọn Pre-heat Over time, Liên hệ Sup để reset.! ");
                    return;
                }
            } 
            else 
            {
                if (machineClickData.length <= 0)
                {
                    if (itemUnselect.length > 0)
                    {
                        for (let i = 0; i < itemUnselect.length; i++) {
                            $("#"+ itemUnselect[i].toString()).css("background-color", colorType.warning);
                            itemUnselect.remove(itemUnselect[i]); 
                        }
                        return;
                    }
                    $(this).removeClass('selected'); 
                    ShowToast(1, "Bạn chọn không đúng máy cần Reset ClickMachine(2)");
                    return;
                }
                if (selectedMchineIdsInUse.length > machineClickData.length)
                {
                    var notInErr = selectedMchineIdsInUse.filter(function(n) {
                        return machineClickData.indexOf(n) < 0;
                    });
                    if (notInErr.length > 0)
                    {
                        $(this).removeClass('selected');
                        ShowToast(1, "Bạn chọn không đúng máy cần Reset ClickMachine(3)");
                        return;
                    }
                }
                //item dang chon co trong list may error
                var erLstFinal = machineClickData.filter(function(n) {
                    return errorListMachine.indexOf(n) !== -1;
                });

                notInItemUnSelect = erLstFinal.filter((element) => { //nhung machine select them,
                    return itemUnselect.indexOf(element) < 0;
                });

                if (erLstFinal.length > itemUnselect.length)
                {
                    for (let i = 0; i < notInItemUnSelect.length; i++) { //to mau machine select them
                        itemUnselect.push(notInItemUnSelect[i]);
                        $("#"+ notInItemUnSelect[i].toString()).css("background-color", colorType.deselected);
                    }
                }
                else 
                {
                    var notInWarning = itemUnselect.filter((element) => { //loc ra machine nao dang bo chon
                        return machineClickData.indexOf(element) < 0;
                    });
                    for (let i = 0; i < notInWarning.length; i++) { //to mau machine do
                        $("#"+ notInWarning[i].toString()).css("background-color", colorType.warning);
                        itemUnselect.remove(notInWarning[i]); 
                    }
                }

                return;
            }
        } 
        //end
        if (machineClickData.length > 0) //click trung may dang chay
        {
            if (itemCheck.length > 0)
            {
                //kiem tra xem co phai click dung may dang check hay khong
                notInItemCheck = [];

                if (machineClickData.length > itemCheck.length) //click them, hoac bo? click
                {
                    var arrayItem = itemCheck.concat(machineClickData); 
                    arrayItem = arrayItem.filter((item, pos) => arrayItem.indexOf(item) === pos) //loc trung
                    
                    notInItemCheck = arrayItem.filter((element) => { //nhung machine select them
                        return itemCheck.indexOf(element) < 0;
                    });
                    
                    for (let i = 0; i < notInItemCheck.length; i++) {
                        itemCheck.push(notInItemCheck[i]);
                        $("#"+ notInItemCheck[i].toString()).css("background-color", colorType.deselected);
                    }
                }
                else //bo chon
                {
                    //loc ra may nao dang bo chon
                    notInItemCheck = itemCheck.filter((element) => {
                        return machineClickData.indexOf(element) < 0;
                    });
                    //bo chon thi remove no ra khoi array item check.
                    for (let i = 0; i < notInItemCheck.length; i++) {
                        $("#"+ notInItemCheck[i].toString()).css("background-color", colorType.selected);
                        itemCheck.remove(notInItemCheck[i]); 
                    }
                }
            }
            else 
            {
                for (let i = 0; i < machineClickData.length; i++) {
                    $("#"+ machineClickData[i].toString()).css("background-color", colorType.deselected);
                    itemCheck.push(machineClickData[i].toString());
                }    
            }
        }
        else 
        {
            if (itemCheck.length > 0)
            {
                for (let i = 0; i < itemCheck.length; i++) {
                    $("#"+ itemCheck[i].toString()).css("background-color", colorType.selected);
                    itemCheck = [];
                }    
            }
        }
    }
    else //khi bo? chon may dau tien
    {
        if (errorListMachine.length > 0)
        {
            for (let k = 0; k< errorListMachine.length; k++) {
                $("#"+ errorListMachine[k].toString()).css("background-color", colorType.warning);
            }
        }
        if (itemUnselect.length > 0)
        {
            for (let j = 0; j < itemUnselect.length; j++) {
                $("#"+ itemUnselect[j].toString()).css("background-color", colorType.warning);
            }
            itemUnselect = [];
        } 
        else if (itemCheck.length > 0)
        {
            for (let i = 0; i < itemCheck.length; i++) {
                $("#"+ itemCheck[i].toString()).css("background-color", colorType.selected);
                itemCheck = [];
            }    
        }
        machineClickData = [];
    } 
    //end
}
//what the  
Array.prototype.remove = function() {
    var what, a = arguments, L = a.length, ax;
    while (L && this.length) {
        what = a[--L];
        while ((ax = this.indexOf(what)) !== -1) {
            this.splice(ax, 1);
        }
    }
    return this;
};

var submitBtn = document.getElementById("BtnAccept");
submitBtn.addEventListener("click", UpdAndInst);

var noPlanAndUser = {
    noPlan: "No Plan",
    none: "None",
    curing: "Running",
    overTime: "Pre-heat Over time"
}

var checkDeselected = [];

function UpdAndInst()
{
    //start 20200715 khi finished work phai trong khung time tu 13->14h, 21h->22h
    var today = new Date();

    var startearlyShiftA = new Date();
    startearlyShiftA.setHours(5, 0, 0); // 5.00 am
    var endtearlyShiftA = new Date();
    endtearlyShiftA.setHours(7, 0, 0); // 7.00 am

    var startShiftA = new Date();
    startShiftA.setHours(13, 0, 0); // 13.00 pm
    var endShiftA = new Date();
    endShiftA.setHours(15, 0, 0); // 15.00 pm

    var startShiftB = new Date();
    startShiftB.setHours(21, 0, 0); // 21.00 pm
    var endShiftB = new Date();
    endShiftB.setHours(23, 0, 0); // 22.00 pm

    if (idValue === 10) //finish work trong khoang thoi gian nay
    {
        if (today < startearlyShiftA && today > endtearlyShiftA) {
            ShowToast(1, "Chưa đến giờ Finish Work 5:00 Am - 7:00 Am");
            return;
        } else if (today > endtearlyShiftA &&  today < startShiftA) {
            ShowToast(1, "Chưa đến giờ Finish Work 13:00 pm");
            return;
        } else if (today > endShiftA && today < startShiftB) {
            ShowToast(1, "Chưa đến giờ Finish Work 13:00 - 15:00 pm");
            return;
        } else if (today > endShiftB) {
            ShowToast(1, "Sai giờ Finish Work 21:00 - 23:00 pm");
            return;
        } 
    }
    //end 20200715 khi finished work phai trong khung time tu 13->14h, 21h->22h
    //check chooses machine
    if (selectedMchineIds == undefined)
    {
         //adjust later
        //alert("Please select machine to execute!!")
        ShowToast(1, "Please select machine to execute!!");
        return;
    }
    if (selectedMchineIds.length <= 0)
    {
         //adjust later
        //alert("Please select machine to execute!!")
        ShowToast(1, "Please select machine to execute!!");
        return;
    }
    //check machine selected
    //filter machine has in td_job_prmold_os to recognize which machine not run.
    var isNoplan = selectedMchineIds.filter((value) => {
        return value.firstElementChild.textContent === noPlanAndUser.none;
    });

    if (isNoplan.length > 0)
    {
         //adjust later
        //alert("Có máy bạn chọn chưa quét chức năng máy đúc.!")
        ShowToast(1, "Máy bạn chọn chưa quét chức năng máy đúc.!");
        LoadMachineToModal();
        return;
    }
    //end

    //console.log(StatusSelectedMachineArr);
    
    //filter machine that you click has the same status
    checkDeselected = [];
    if (machineClickData.length > 0)
    {
        for (let i = 0; i < selectedMchineIds.length; i++) { 
            if (selectedMchineIds[i].lastElementChild.textContent.toString().trim() !== noPlanAndUser.noPlan)
            {
                checkDeselected.push(selectedMchineIds[i].lastElementChild.textContent.toString().trim());
            }
        }
        if (checkDeselected.length > 0)
        {
            if (selectedMchineIds.length !== checkDeselected.length)
            {
                ShowToast(1, "Có máy bạn chọn chưa chọn không cùng trạng thái [1]");
                LoadMachineToModal();
                return;
            }
        }
        if (selectedMchineIds.length > 1) //2 phan tu tro len moi check
        {
            let uniqueStatus = checkDeselected.filter((item, i, ar) => ar.indexOf(item) === i);
            if (uniqueStatus.length > 1) //nhieu hon 1 phan tu nghia la da chon 2 trang thai khac nhau
            {
                //alert("Có máy bạn chọn chưa chọn không cùng trạng thái")
                ShowToast(1, "Có máy bạn chọn chưa chọn không cùng trạng thái");
                LoadMachineToModal();
                return;
            }
        }
        //may click co trong cac may co trang thai da chon hay khong
        //check
        for (let i = 0; i < StatusSelectedMachineArr.length; i++) {
            for (let j = 0; j < machineClickData.length; j++) {
                if (StatusSelectedMachineArr[i].pressNo === machineClickData[j])
                {
                    if (parseInt(StatusSelectedMachineArr[i].status) !== 1)
                    {
                        if (parseInt(StatusSelectedMachineArr[i].status) !== idValue)
                        {
                            ShowToast(1, "Bạn đã chọn sai nội dung")
                            //alert("Bạn đã chọn sai nội dung")
                            LoadMachineToModal();
                            return;
                        }
                    }
                }
            }
        }
        //get data save
        var finalData = machineClickData.filter(function(obj) { 
            return StatusSelectedMachineArr.indexOf(obj) == -1; 
        });
    }
    //end

    //chac chan da cung trang thai
    //1. khi co trang thai
    var obj;
    var datas;
    for (let i = 0; i < finalData.length; i++) {
        if (finalData[i] === selectedMchineIds[i].id)
        {
            datas = selectedMchineIds.map((value) => {
                obj = {
                    id : value.id,
                    content : value.lastElementChild.textContent,
                    user: value.firstElementChild.textContent
                }
                return obj;
            });
        }
    }
    var objData = [];
    //execute data here, update status
    for (let i = 0; i < datas.length; i++) {
        var Data = {
            "lineNo": gLineNo,
            "pressNo": datas[i].id.toString().trim(),
            "value": idValue,
            "users": datas[i].user.toString().trim(),
            "errType" : ""
        };
        objData.push(Data);
    }
    //execute save data
    $.ajax({
        url: '/Home/UpdAndInsrt',
        type: "POST",
        contentType : "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(objData), 
        // fetch machine
        success: function (result) { 
            if (result.success === true) {
                 //adjust later
                //alert("Upd data successful")
                ShowToast(0, 0);
                LoadMachineToModal();
            } else {
                if (result.type == 1) {
                    var errData = JSON.parse(result.error);
                    var errWashing = JSON.parse(result.errWashing);

                    if (errData.length > 0) {
                        //20200901
                        if (errWashing.length > 0) {
                            for (let i = 0; i < errWashing.length; i++) {
                                ShowToast(2, "Máy " + errWashing[i] + " đang rửa khuôn"); //error from server response
                            }
                        }
                        //end 20200901
                        for (let i = 0; i < errData.length; i++) {
                            ShowToast(2, "Máy " + errData[i] + " chưa Pre-heat"); //error from server response
                        }
                        LoadMachineToModal();
                        return;
                    }
                    //20200901
                    if (errWashing.length > 0) {
                        for (let i = 0; i < errWashing.length; i++) {
                            ShowToast(2, "Máy " + errWashing[i] + " đang rửa khuôn"); //error from server response
                        }
                        LoadMachineToModal();
                        return;
                    }
                    //end 20200901
                } else {
                    ShowToast(2, result.error); //error from server response
                    LoadMachineToModal();
                    return;
                }
            }
            selectedMchineIds = [];
        },
        error: function(errormessage) {
             //adjust later
             RefreshArr();
             ShowToast(2, "UpdAndInsrt() error..") ;
        }
    });
    //end execute data
}

// rest sup xong rest lai gUserSup
var btnReset = document.getElementById("BtnRstSup");
btnReset.addEventListener("click", SupervisorConfirm);

var reasonMenu = document.getElementById("RstMenu");

function SupervisorConfirm()
{
    //machineClickData
    //ResetMachine
    if (reasonMenu.text.trim() === "Chooses reason") {
        ShowToast(1, "Vui lòng chọn nội dung reset");
        return;
    }

    if (errorListMachine.length <= 0)
    {
        ShowToast(1, "Không có máy nào đang bị lỗi");
        return;
    }

    if (selectedMchineIds == undefined)
    {
        ShowToast(1, "Please select machine to execute!! SupervisorConfirm()");
        return;
    }
    if (selectedMchineIds.length <= 0)
    {
        ShowToast(1, "Please select machine to execute 1!! SupervisorConfirm()");
        return;
    }

    var obj;
    var datas;
    for (let i = 0; i < machineClickData.length; i++) {
        datas = selectedMchineIds.map((value) => {
            obj = {
                id : value.id,
                content : value.lastElementChild.textContent,
                user: gUserSup
            }
            return obj;
        });
    }

    var objData = [];

    var errType = reasonMenu.text.trim().substring(0, 2).replace('.', '');
    //execute data here, update status
    for (let i = 0; i < datas.length; i++) {
        var Data = {
            "lineNo": gLineNo,
            "pressNo": datas[i].id.toString().trim(),
            "value": "14",
            "users": datas[i].user.toString().trim(),
            "errType": errType
        };
        objData.push(Data);
    }
    
    $.ajax({
        url: '/Home/ResetMachine',
        type: "POST",
        contentType : "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(objData), 
        // fetch machine
        success: function (result) { 
            if (result.success === true) {
                 //adjust later
                //alert("Upd data successful")
                ShowToast(0, 0);
                errorListMachine = [];
                LoadMachineToModal();
            } else {
                ShowToast(2, result.error); //error from server response
                return;
            }
            selectedMchineIds = []; //reset
        },
        error: function(errormessage) {
             //adjust later
             RefreshArr();
            ShowToast(2, "SupervisorConfirm() error..") ;
        }
    });
}
   
//type: success, warning, error
//error: what kind of error ?
function ShowToast(type, err) 
{
    //invoke alert
    var alertbox = new AlertBox('#alert-area', {
        closeTime: 5000,
        persistent: false,
        hideCloseButton: false
    });
    if (type == 0)
    {
        $('#alert-area').css("background", "green")
        alertbox.show("Successful! ");
    } 
    else if (type == 1)  
    {
        $('#alert-area').css("background", "DodgerBlue")
        alertbox.show("Information! " + err);
    }
    else 
    {
        $('#alert-area').css("background", "#DC143C")
        alertbox.show("Warning! " + err);
    }
}
// The main class alert, show alert like snack bar =)))
var AlertBox = function(id, option) {
this.show = function(msg) {
    if (msg === ''  || typeof msg === 'undefined' || msg === null) {
        throw '"msg parameter is empty"';
    }
    else {
        var alertArea = document.querySelector(id);
        var alertBox = document.createElement('DIV');
        var alertContent = document.createElement('DIV');
        var alertClose = document.createElement('A');
        var alertClass = this;

        alertContent.classList.add('alert-content');
        alertContent.innerText = msg;
        alertClose.classList.add('alert-close');
        alertClose.setAttribute('href', '#');
        alertBox.classList.add('alert-box');
        alertBox.appendChild(alertContent);

        if (!option.hideCloseButton || typeof option.hideCloseButton === 'undefined') {
            alertBox.appendChild(alertClose);
        }
        alertArea.appendChild(alertBox);
        alertClose.addEventListener('click', function(event) {
            event.preventDefault();
            alertClass.hide(alertBox);
        });
        if (!option.persistent) {
            var alertTimeout = setTimeout(function() {
                alertClass.hide(alertBox);
                clearTimeout(alertTimeout);
            }, option.closeTime);
        }
    }
};

this.hide = function(alertBox) {
    alertBox.classList.add('hide');
    var disperseTimeout = setTimeout(function() {
        alertBox.parentNode.removeChild(alertBox);
        clearTimeout(disperseTimeout);
        }, 500);
    };
};
  

//end 20200520


/* Whatsapp Chat */

$(document).on("click", ".close-chat", function() {
    $("#whatsapp-chat").addClass("pophide").removeClass("popshow")
}), $(document).on("click", ".blantershow-chat", function() {
    $("#whatsapp-chat").addClass("popshow").removeClass("pophide")
});

