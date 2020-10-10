var oTable;

$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl('/moldshub').build();
    connection.on('ReceiveMessage', function (message) {
        // $('#table_id').reload();
        var encodedMsg = JSON.parse(message);
        // Add the message to the page.

        if ($.fn.DataTable.isDataTable("#table_id")) {
            oTable.clear();
            oTable.rows.add(encodedMsg.lstMoldWashing);
            oTable.draw();
        }
        else {
            oTable = $('#table_id').DataTable({
                processing: true,
                select: true,
                paging: true,
                sort: false,
                searching: true,
                responsive: true,
                data: encodedMsg.lstMoldWashing,
                columns: [
                    { data: 'wc' },
                    { data: 'material_mark1' },
                    { data: 'type_name' },
                    { data: 'mold' },
                    { data: 'mold_job' },
                    {
                        data: 'machine_set_up_sign',
                        render: function (machine_set_up_sign) {
                            if (machine_set_up_sign == 2) {
                                return '<td><div>YES</div></td>'
                            } else {
                                return '<td><div>-</div></td>'
                            }
                        }
                    },
                    {
                        data: 'status',
                        render: function (status) {
                            if (status == 1) {
                                return '<td><div class="label label-other" style="border-radius: 5px; background-color: #5bc0de; color: white; border-color: white; border: solid 0.5px;">Prepare Wash</div></td>'
                            } else if (status == 4) {
                                return '<td><div class="label label-warning" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Spare</div></td>'
                            }
                            else if (status == 6) {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Receive</div></td>'
                            }
                            else if (status == 7 || status == 15) {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Washing<div></td>'
                            }
                            else if (status == 8 || status == 16) {
                                return '<td><div class="label label-success" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Finished<div></td>'
                            }
                            else if (status == 9) {
                                return '<td><div class="label label-success" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Delivery<div></td>'
                            }
                            else if (status == 10 || status == 11 || status == 12) {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Inspection</div></td>'
                            }
                            else if (status == 13) {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Wash Oil Mold</div></td>'
                            }
                            else if (status == 14) {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Production return</div></td>'
                            }
                            else {
                                return '<td><div class="label label-info" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Production start</div></td>'
                            }
                        }
                    },
                    { data: 'entry_date' },
                    { data: 'entry_time' },
                    { data: 'receive_date' },
                    { data: 'receive_time' },
                    { data: 'start_time' },
                    { data: 'estimate_time' },
                    { data: 'end_time' },
                    { data: 'delivery_time' },
                    {
                        data: 'judgment',
                        render: function (judgment) {
                            if (judgment == "Delay") {
                                return '<td><div class="label label-danger" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Delay</div></td>'
                            }
                            else if (judgment == "Good") {
                                return '<td><div class="label label-success" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Good</div></td>'
                            }
                            else if (judgment == "Not Good") {
                                return '<td><div class="label label-warning" style="border-radius: 5px; color: white; border-color: white; border: solid 0.5px;">Not Good</div></td>'
                            } else {
                                return '<td><div class="label label-other" style="border-radius: 5px; background-color: #fcbd0c; color: white; border-color: white; border: solid 0.5px;">Judging</div></td>'
                            }
                        }
                    },
                ],
            });
        }
        var table = $('#table_id').DataTable();
        $('#table_id tbody').on('click', 'tr', function () {
            var data = table.row(this).data();
            console.log(data);

            //$('#moldDetails').modal('show'); //future
        });
    });

    // Transport fallback functionality is now built into start.
    connection.start()
        .then(function () {
            console.log('Connection started');
            connection.invoke('SendMessage');
        })
        .catch(error => {
            console.error(error.message);
        });
});