# CardGrid 1.0
Grid mixed with the features of Card and Table Views
This jQuery plugin is to create a Grid Control with Card and Table based on JSON Data and custom templates for Cards. This is Designed to use with [Bootstrap 3](http://getbootstrap.com) or more

![Screenshot](screenshots/cardgrid_Employee_ScreenShot.png)
![Screenshot](screenshots/cardgrid_Employee_TableView.png)

## Help In Project
Currently Project is deployed with ASP.NET MVC Project. This plugin is written in TypeScript. Source code is available in typescript folder.
## Required
* [jQuery](http://jquery.com/)
* [Bootstrap](http://getbootstrap.com/)
* [BootPag](http://botmonster.com/jquery-bootpag/#.WQYfe7puKhc)
* [Font-aweasome](http://fontawesome.io/)
## Author
The __CardGrid plugin__ is written by Ashutosh Nigam, @ashutoshrewa
* [ashutoshnigam.com](http://www.ashutoshnigam.com/)
* [Facebook](https://www.facebook.com/ashutosh.nigam2)
* [Twitter](https://twitter.com/ashutoshrewa)
## Getting Started

* Create a Div with name "card-grid
    ```
           <div class="col-lg-12 col-sm-12 col-md-12 col-xs-12">
               <div class="card-grid">
               </div>
           </div>
    ```
 * Add Css Files
    ```
    <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" type="text/css" />   
    <script src="~/Scripts/modernizr-2.6.2.js"></script>  
    <link href="~/Content/cardgrid.css" rel="stylesheet" type="text/css" />
    ```
 * Add Javascript File  
     ``` 
    <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" type="text/css" />   
    <script src="~/Scripts/modernizr-2.6.2.js"></script>  
    <link href="~/Content/cardgrid.css" rel="stylesheet" type="text/css" />
    <script src="~/Scripts/jquery-3.1.1.min.js"></script>
    <script src="~/Scripts/bootstrap.min.js"></script>
    <script src="~/Scripts/jquery.bootpag.min.js"></script>
    <script src="~/Scripts/App/CardGrid.js"></script>
     ```
  * Add Script in Page
     ``` 
    var cardGrid = null;
    $(document).ready(function () {       
        cardGrid = $('.card-grid').cardGrid({
            url: '@Url.Action("GetData")',
            type: 'GET',
            pagination: true,
            itemsPerRow: 4,
            recordsPerPage: [12, 24, 48],
            sortCols: [
                { text: 'Id', value: 'Id' },
                { text: 'Name', value: 'Name' },
                { text: 'Gender', value: 'Gender' },
                { text: 'Date Of Birth', value: 'BirthDate' },
                { text: 'Contact No', value: 'ContactNo' },
                { text: 'Email', value: 'Email' },
                { text: 'Job Title', value: 'JobTitle' },
                { text: 'Date of Hiring', value: 'HireDate' },
            ],
            currentView: 'card',
            multiSelect: false,
            cols: [
                { id: 'Id', title: 'Id', hidden: true },
                { id: 'Name', title: 'Name' },
                { id: 'Gender', title: 'Gender', width: 6},
                {
                    id: 'BirthDate', title: 'Date of Birthday', template: function (data) {
                        return (new Date(parseInt(data.BirthDate.replace(/\/+Date\(([\d+-]+)\)\/+/, '$1')))).toDateString();
                    }
                },
                { id: 'ContactNo', title: 'Contact No'},
                { id: 'Email', title: 'Email',  },
                { id: 'JobTitle', title: 'Job Title' },
                {
                    id: 'HireDate', title: 'Date of Hire', template: function (data) {
                        return (new Date(parseInt(data.HireDate.replace(/\/+Date\(([\d+-]+)\)\/+/, '$1')))).toDateString();
                    }}
            ],
            template: function (data) {
                var _div_Row1 = $('<div/>');
                var _div_Row2 = $('<div/>');
                var _div_Row3 = $('<div/>');
                var _img = $('<img />', { src: 'https://unsplash.it/150/100?image=' + data.Id, style: 'width:150px;height:100px' });
                var _name = $('<h3/>', { html: data.Name,style:'background: antiquewhite;'});
                _div_Row1.append(_img);
                _div_Row2.append(_name);
                
                _div_Row3.append($('<h5/>', { html: data.Gender }))
                    .append($('<h5/>', { html: (new Date(parseInt(data.BirthDate.replace(/\/+Date\(([\d+-]+)\)\/+/, '$1')))).toDateString() }))
                    .append($('<h5/>', { html: data.ContactNo }))
                    .append($('<h5/>', { html: data.Email }))
                    .append($('<h5/>', { html: data.JobTitle }))
                    .append($('<h5/>', { html: (new Date(parseInt(data.HireDate.replace(/\/+Date\(([\d+-]+)\)\/+/, '$1')))).toDateString() }));

                return $('<div />', { style: 'text-align:center' }).append(_div_Row1).append(_div_Row2).append(_div_Row3);
            },
            buttons: [
                {
                    id: 'btnAdd', text: '', enable: true, hidden: false, className: 'btn-success fa fa-plus', onClick: function () {
                        var modal = $('#myModal').modal();
                       // Write Code to Submit Your Code
                    }
                },
                {
                    id: 'btnEdit', text: '', className: 'fa fa-pencil', onClick: function () {
                        var gridData = cardGrid.getSelectedData();
                        if (gridData.length == 0) {
                            alert('Please select an Item to Edit!');
                            return;
                        }
                        if (gridData.length > 1) {
                            alert('Please Select single Item to Edit');
                            return;
                        }
                        var modal = $('#myModal').modal();
                        // Write Code to Submit Your Code
                    }
                },
                {
                    id: 'btndelete', text: '', className: 'btn-danger fa fa-trash', onClick: function () {
                        var selectedIds = cardGrid.getSelectedIds();
                        if (selectedIds.length === 0) {
                            alert('Please select atleast one Item to Delete!');
                            return;
                        }
                        // Write Code to Submit Your Code

                    }
                },
                {
                    text: 'Print', onClick: function () {
                        window.print();
                    }, buttons: [
                        {
                            text: '', hidden: true, onClick: function () {
                                alert('Edit- Excel 1')
                            }
                        },
                        {
                            text: 'Excel', onClick: function () {
                                alert("excel");
                            }
                        },
                    ]
                }
            ]
        });
    });
    
     ```
