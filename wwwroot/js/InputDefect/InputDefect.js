$(document).ready(function() {
    var gbBarcode = document.getElementById("txtBarcode");
    var gbFinishedGoodsCode, gbLotNo, gbJobOrderNo, gbLineNo, gbMaterialMark, gbRotation, gbCuringQty, gbCavityQty;
    var gbOperationSeq, gbPressNo, gbLineNoCheck, gbCuringDate, gbErrorList;

    // document.getElementById("btnSubmit").disabled = true;

    function openLoader() {
        document.getElementById('loader-4').style.display = 'block';
        //document.getElementById('fade').style.display = 'block';
    }

    function closeLoader() {
        document.getElementById('loader-4').style.display = 'none';
        //document.getElementById('fade').style.display = 'none';
    }

    gbBarcode.addEventListener("change", GetDataByJobNo);

    /*get data by job order no
    show info. on the left view*/
    function GetDataByJobNo() {
        var jobOrderNo = $("#txtBarcode").val();
        //uncheck all checkbox
        $('input[type="checkbox"]').prop({
            checked: false,
            disabled: true
        });
        document.getElementById("btnSubmit").disabled = false;
        openLoader();
        $.ajax({
            url: "GetDataByJobNo",
            type: "GET",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: {
                jobOrderNo: jobOrderNo,
            },
            success: function(result) {
                if (!result.success) {
                    $(".form-control").val("");
                    $("#ddlProcess").empty();
                    $("#fs-error-list").remove();
                    $("#operation-number").val("");
                    $("#txtJobOrderNo").focus();
                    $("#txtNotes").val("");
                    $.notify("Job tag không tìm thấy dữ liệu lỗi", "error");
                    closeLoader();
                    return;
                }

                var htmlProcess = "";
                var month;
                gbCuringDate = result.data[0].curing_date == null ? "" : result.data[0].curing_date;
                month = getMonthByCurDate(gbCuringDate);
                gbJobOrderNo = result.data[0].job_order_no.toString();
                gbFinishedGoodsCode = result.data[0].finished_goods_code.toString();

                gbLotNo = gbCuringDate.substring(3, 4) + month + gbCuringDate.substring(6, 8) + " " + result.data[0].lot_no.toString();
                gbLineNoCheck = result.data[0].line_no.toString() == null ? "" : result.data[0].line_no.toString(); //20191113 comment out
                gbLineNo = gbLineNoCheck.substring(0, gbLineNoCheck.indexOf("/")); //20191113 add line no, press no
                gbPressNo = gbLineNoCheck.substring(gbLineNoCheck.indexOf("/") + 1, 5);
                gbMaterialMark = result.data[0].material_mark1.toString() == null ? "" : result.data[0].material_mark1.toString();
                gbRotation = result.data[0].rotation == null ? 0 : result.data[0].rotation;
                gbCuringQty = result.data[0].curing_qty == null ? 0 : result.data[0].curing_qty;
                gbCavityQty = result.data[0].cavity_qty == null ? 0 : result.data[0].cavity_qty;

                $("#txtJobOrderNo").val(gbJobOrderNo);
                $("#txtItemCode").val(gbFinishedGoodsCode);
                $("#txtLotNo").val(gbLotNo);

                // generating process drop-down list
                $.each(result.data, function(index, item) {
                    htmlProcess += '<option value = "' + item.operation_code.trim().toString() + '">' + item.operation_name.trim().toString() + "</options>";
                });
                //20191205 add dropdownlist 'Kiem Lua Lai'
                htmlProcess += '<option value = "999">KIEM LUA LAI</options>';

                $("#ddlProcess").html(htmlProcess);
                $("#txtBarcode").val("");
                //load error list by operation code
                if ($("#ddlProcess")[0].selectedIndex == 0) {
                    var jobOrderNo = $("#txtJobOrderNo").val();
                    var operationCode = $("#ddlProcess").val();
                    GetErrListByOperationCode(jobOrderNo, operationCode);
                }
                closeLoader();
                $("#txtQuantity").val("");
                $("#txtQuantity").focus();
                $("#txtNotes").val("");
            },
            error: function(errormessage) {
                $.notify(errormessage.responseText, "error");
            },
        });
    }

    //get error list by operation code
    function GetErrListByOperationCode(jobOrderNo, operationCode) {
        //check so lan thao tac
        document.getElementById("btnSubmit").disabled = false;
        //clear all
        $('input[type="checkbox"]').prop({
            checked: false,
            disabled: true
        });
        $("#error-data-list01").html('');
        $("#error-data-list02").html('');

        $.ajax({
            url: "GetErrListByOperationCode",
            type: "GET",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: {
                jobOrderNo: jobOrderNo,
                operationCode: operationCode,
            },
            success: function(result) {
                if (result.data.length == 0) {
                    $.notify("Chưa ghi nhận chống sót công đoạn", "error");
                    return;
                }

                gbErrorList = result.data;
                //operation-number: số lần thao tác
                var number = result.operationNumber.length == 0 ? 0 : result.operationNumber[0].operationNumber;
                document.getElementById("operation-number").innerText = number;

                var htmlErrCol01 = "";
                var htmlErrCol02 = "";
                var htmlErrTotal = "";
                var receiveStartDate = result.data[0].start_date.toString("yyyy-MM-dd").trim() == "" ? null : result.data[0].start_date.toString("yyyy-MM-dd").trim();
                var receiveEndDate = result.data[0].end_date.toString("yyyy-MM-dd").trim() == "" ? null : result.data[0].end_date.toString("yyyy-MM-dd").trim();

                var startDate = receiveStartDate == null ? "" : receiveStartDate.substring(0, 4) + "-" + receiveStartDate.substring(4, 6) + "-" + receiveStartDate.substring(6, 8);
                var endDate = receiveEndDate == null ? "" : receiveEndDate.substring(0, 4) + "-" + receiveEndDate.substring(4, 6) + "-" + receiveEndDate.substring(6, 8);

                document.getElementById("txtStartDate").value = startDate;
                document.getElementById("txtEndDate").value = endDate;
                gbOperationSeq = result.data[0].operation_sequence;

                var machine = result.data[0].machine_no == null ? "" : result.data[0].machine_no;
                $("#txtMachineNo").val(machine);

                var nameID = 0;
                var nextNameID = 0;
                var nameNotes = document.getElementById("txtNotes").name;

                $.each(result.data, function(key, item) {
                    nameID += 1;
                    nextNameID = nameID + 1;
                    //only display 10 errors per column
                    if (key < 10) {
                        //error list column 1
                        htmlErrCol01 += '<div class="ipd-form-group row">';
                        htmlErrCol01 += '<label class="col-sm-6 col-form-label ipd-label-error"> ';
                        htmlErrCol01 += nameID + ". " + item.error_name;
                        htmlErrCol01 += "</label>";
                        htmlErrCol01 += '<div class="col-sm-6 row">';

                        if (nameID === result.data.length) {
                            htmlErrCol01 += '<input id="' + item.error_id.trim() + '" class="form-control ipd-input-error"  name="' + nameID + '"  onkeyup="enter(event,' + nameNotes + ');" autocomplete="off" />';
                        } else {
                            htmlErrCol01 += '<input id="' + item.error_id.trim() + '" class="form-control ipd-input-error"  name="' + nameID + '"  onkeyup="enter(event,' + nextNameID + ');" autocomplete="off" />';
                        }

                        htmlErrCol01 += '<input id="P' + item.error_id.trim() + '" class="form-control ipd-percent-error" type="text" disabled>';
                        htmlErrCol01 += "</div>"; //end <div class="ipd-form-group row">
                        htmlErrCol01 += "</div>"; //<div class="col-sm-6 row">
                    } else {
                        //error list column 2
                        htmlErrCol02 += '<div class="ipd-form-group row">';
                        htmlErrCol02 += '<label class="col-sm-6 col-form-label ipd-label-error"> ';
                        htmlErrCol02 += nameID + " ." + item.error_name;
                        htmlErrCol02 += "</label>";
                        htmlErrCol02 += '<div class="col-sm-6 row">';

                        if (nameID === result.data.length) {
                            htmlErrCol02 += '<input id="' + item.error_id.trim() + '" class="form-control ipd-input-error"  name="' + nameID + '"  onkeyup="enter(event,' + nameNotes + ');"  autocomplete="off" />';
                        } else {
                            htmlErrCol02 += '<input id="' + item.error_id.trim() + '" class="form-control ipd-input-error"  name="' + nameID + '"  onkeyup="enter(event,' + nextNameID + ');" autocomplete="off" />';
                        }

                        htmlErrCol02 += '<input id="P' + item.error_id.trim() + '" class="form-control ipd-percent-error" type="text" disabled>';
                        htmlErrCol02 += "</div>"; //end <div class="ipd-form-group row">
                        htmlErrCol02 += "</div>"; //<div class="col-sm-6 row">
                    }
                });
                //error total
                htmlErrTotal += '<div class="ipd-form-group row">';
                htmlErrTotal += '<label class="col-sm-6 col-form-label ipd-label-error"> ';
                htmlErrTotal += "TOTAL";
                htmlErrTotal += "</label>";
                htmlErrTotal += '<div class="col-sm-6 row">';
                htmlErrTotal += '<input id="total" class="form-control ipd-percent-error" type="text" disabled>';
                htmlErrTotal += '<input id="Ptotal" class="form-control ipd-percent-error" type="text" disabled >';
                htmlErrTotal += "</div>"; //end <div class="ipd-form-group row">
                htmlErrTotal += "</div>"; //<div class="col-sm-6 row">'
                //generating error list
                $("#error-data-list01").html(htmlErrCol01);
                $("#error-data-list02").html(htmlErrCol02);

                if (htmlErrCol02 == "")
                    $("#error-data-list01").append(htmlErrTotal);
                else
                    $("#error-data-list02").append(htmlErrTotal);

                //check
                $('input[class^="form-control ipd-input-error"]').each(function(key, item) {
                    $("#" + item.id).on("input change", changeEvent);

                    function changeEvent(e) {
                        var showAlert = true;
                        var processQty = document.getElementById("txtQuantity").value;

                        if (showAlert) {
                            if (processQty == "") {
                                $("#txtQuantity").notify("Hãy nhập số lượng");
                                $(this).val("");
                                $("#txtQuantity").focus();
                                showAlert = false;
                                return;
                            }
                        }
                        //var targetValue = e.target.value;
                        var percentValue;
                        percentValue = ((e.target.value * 100) / $("#txtQuantity").val()).toFixed(2);
                        $("#P" + item.id).val(percentValue + " %");
                        //refresh txt percent
                        if (e.target.value.length == 0) {
                            $("#P" + item.id).val('');
                        }

                        var otherError = $('#016').val();
                        if (otherError > 0) {
                            $('input[type="checkbox"]').prop({
                                disabled: false
                            });
                        } else {
                            $('input[type="checkbox"]').prop({
                                disabled: true,
                                checked: false
                            });
                            $('#txtNotes').val('');
                        }
                    }

                    $("#" + item.id).keyup(function(e) {
                        sumErrTotal();
                    });
                });
            },
            error: function(errormessage) {
                $.notify(errormessage.responseText, "error");
            },
        });
    }

    //calculate the total error
    function sumErrTotal() {
        var sum = 0;
        var pValue;
        $('input[class^="form-control ipd-input-error"]').each(function(key, item) {
            if (!isNaN(this.value) && this.value.length != 0) {
                sum += parseFloat(this.value);
            }
        });

        if (sum >= 0) {
            $("#total").val(sum);
            pValue = (($("#total").val(sum).val() * 100) / $("#txtQuantity").val()).toFixed(2);
            $("#Ptotal").val(pValue + ' %');
        }
        // else {
        //     $("#total").val('');
        //     $("#Ptotal").val('');
        // }

    }


    $("#ddlProcess").on("change", selectedIndexChanged);

    function selectedIndexChanged() {
        var operationCode = $(this).children("option:selected").val();
        var jobOrderNo = $("#txtJobOrderNo").val();

        $("#txtQuantity").val("");
        $("#txtNotes").val("");
        //uncheck all checkbox
        //$('input[type=checkbox]').prop('checked', false);
        GetErrListByOperationCode(jobOrderNo, operationCode);
        $("#txtQuantity").focus();
    }


    $("#confirmModal").on("shown.bs.modal", function(event) {
        $("#btnSave").focus();
    });

    $("#continueModal").on("shown.bs.modal", function(event) {
        $("#btnContinue").focus();
    });

    $("#btnSubmit").click(function(e) {
        var showAlert = true;
        var okQty = document.getElementById("txtQuantity").value;
        var jobNo = document.getElementById("txtJobOrderNo").value;

        if (jobNo == "") {
            $('#txtBarcode').notify(
                "Hãy quét job tag"
            );
            return false;
        }

        if (okQty == "") {
            $('#txtQuantity').focus();
            $("#txtQuantity").notify("Hãy nhập số lượng");
            return false;
        }

        //kiem tra da nhap day du thong tin chua
        $('input[class^="form-control ipd-input-error"]').each(function(key, value) {
            if ($("#" + value.id).val() == null || $("#" + value.id).val() == "") {
                $("#" + value.id).notify("Hãy nhập số lượng");
                $("#" + value.id).focus();
                showAlert = false;
                return false;
            }
        });

        //#016: lỗi khác
        if ($("#016").val() != 0) {
            if (!checkChecked("otherErrForm")) {
                showAlert = false;
                return false;
            }
        } else {
            $("#txtNotes").val("");
        }

        if (showAlert == true) {
            $("#confirmModal").modal("show");
        }

    });

    function checkChecked(formName) {
        var anyBoxesChecked = false;
        var notes = '';
        $('#' + formName + ' input[type="checkbox"]').each(function() {
            if ($(this).is(":checked")) {
                anyBoxesChecked = true;
                notes += $(this).val() + ','
            }
        });

        if (anyBoxesChecked == false) {
            $("#lgErrForm").notify(
                "Hãy chọn ghi chú lỗi khác", { position: "left" }
            );
            return false;
        }
        $('#txtNotes').val(notes.slice(0, -1)); //cắt dấu phẩy cuối
        return true;
    }

    $("#btnNoContinue").click(function(e) {
        window.location.assign("/Home/Index");
    });

    $("#btnContinue").click(function(e) {
        $("#continueModal").modal("hide");

        $("#txtQuantity").val("");
        $("#txtNotes").val("");
        $('input[type="checkbox"]').prop({
            checked: false,
            disabled: true
        });
        $("#error-data-list01").html('');
        $("#error-data-list02").html('');
    });

    $("#btnSave").click(function(e) {
        e.preventDefault();
        var objectData = [];
        var today = new Date();
        var curMonth = today.getMonth() + 1 < 10 ? "0" + (today.getMonth() + 1) : today.getMonth() + 1;
        var curDay = today.getDate() < 10 ? "0" + today.getDate() : today.getDate();
        var date = today.getFullYear() + curMonth.toString() + curDay.toString();

        var curHour = today.getHours > 12 ? today.getHours() - 12 : today.getHours() < 10 ? "0" + today.getHours() : today.getHours();
        var curMinute = today.getMinutes() < 10 ? "0" + today.getMinutes() : today.getMinutes();
        var curSeconds = today.getSeconds() < 10 ? "0" + today.getSeconds() : today.getSeconds();

        var time = curHour + "" + curMinute + curSeconds;

        var okQty = document.getElementById("txtQuantity").value;
        if (okQty == "") {
            $("#txtQuantity").notify("Hãy nhập số lượng");
            $('#txtQuantity').focus();
            return false;
        }
        var number = document.getElementById("operation-number").innerText;
        var OperationNumber = parseInt(number) + 1;
        var showAlert = true;
        //20200331
        var jobCheck = document.getElementById("txtJobOrderNo").value;
        var itemCheck = document.getElementById("txtItemCode").value;
        if (jobCheck == "") {
            //showAlertError("<strong>Error!</strong> KHÔNG TÌM THẤY JOBTAG, VUI LÒNG QUÉT LẠI JOBTAG");
            showAlert = false;
            return false;
        }
        if (itemCheck == "") {
            //showAlertError("<strong>Error!</strong> KHÔNG TÌM THẤY ITEM CODE, VUI LÒNG QUÉT LẠI JOBTAG");
            showAlert = false;
            return false;
        }
        //kiem tra da nhap day du thong tin chua
        $('input[class^="form-control ipd-input-error"]').each(function(key, value) {
            if ($("#" + value.id).val() == null || $("#" + value.id).val() == "") {
                //showAlertError("<strong>Error!</strong> BẠN CHƯA NHẬP ĐẦY ĐỦ THÔNG TIN");
                showAlert = false;
                return false;
            }

            var objEror = gbErrorList.find((errorId) => errorId.error_id.trim() === value.id);

            var item = {
                JobOrderNo: document.getElementById("txtJobOrderNo").value,
                OperationNumber: OperationNumber,
                FinishedGoodsCode: document.getElementById("txtItemCode").value,
                LotNo: document.getElementById("txtLotNo").value,
                CavityQty: gbCavityQty,
                LineNo: gbLineNo,
                RubberName: gbMaterialMark,
                PlanCycle: gbRotation,
                PlanQty: gbCuringQty,
                UnitCost: 0,
                UnitPrice: 0,
                // JobStartDate: orderStartDate,
                // JobEndDate: orderEndDate,
                JobStartDate: "",
                JobEndDate: "",
                OperationStartDate: document.getElementById("txtStartDate").value == "" ? "" : document.getElementById("txtStartDate").value.replace(/-/g, ""),
                OperationEndDate: document.getElementById("txtEndDate").value == "" ? "" : document.getElementById("txtEndDate").value.replace(/-/g, ""),
                MachineNo: document.getElementById("txtMachineNo").value,
                OkQty: okQty == null ? 0 : parseInt(document.getElementById("txtQuantity").value),
                ProgressOperationCode: $("#ddlProcess").children("option:selected").val(),
                ProgressOperationSeq: gbOperationSeq,
                ProgressOperationName: $("#ddlProcess").children("option:selected").text(),
                ErrorID: value.id,
                ErrorName: objEror.error_name,
                ErrorNameJP: objEror.error_name_jp,
                ErrorQty: $("#" + value.id).val(),
                Notes: document.getElementById("txtNotes").value == "" ? "" : document.getElementById("txtNotes").value,
                EntryDate: date,
                EntryTime: time,
                EntryUser: document.getElementById("txtUserId").value,
                UpdateDate: "",
                UpdateTime: "",
                UpdateUser: "",
                Status: "0",
                ErrorNameEn: objEror.error_name_en,
                curingDate: gbCuringDate,
                department: objEror.department,
                area: objEror.area,
                programID: objEror.program_id,
                PressNo: gbPressNo,
            };
            objectData.push(item);
        });

        if (showAlert == true) {
            $.ajax({
                url: "SaveData",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(objectData),
                success: function(result) {
                    var html = "";
                    if (result.success) {
                        $("#continueModal").modal("show");
                        $("#confirmModal").modal("hide");
                        return true;
                    } else {
                        $.notify("Lưu dữ liệu thất bại", "error");
                        return false;
                    }
                },
                error: function(errormessage) {
                    $.notify(errormessage, "error");
                    return false;
                },
            });
        }
    });
});

// function showAlertError(info) {
//     //'<strong>Error!</strong> Hãy nhập mã số nhân viên'
//     $("#txtMessage").html(info);

//     $("#myAlert").show("fade");

//     setTimeout(function() {
//         $("#myAlert").hide("fade");
//     }, 4000);
// }

function enter(event, number) {
    if (event.keyCode == 13) {
        var wow = document.getElementsByName(number);
        wow[0].focus();
    }
}

function getMonthByCurDate(curDate) {
    var month;
    if (curDate.substring(4, 6) == "10") {
        month = "X-";
    } else if (curDate.substring(4, 6) == "11") {
        month = "Y-";
    } else if (curDate.substring(4, 6) == "12") {
        month = "Z-";
    } else {
        month = curDate.substring(5, 6) + "-";
    }
    return month;
}