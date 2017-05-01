# CardGrid 1.0
Grid mixed with the features of Card and Table Views
This jQuery plugin is to create a Grid Control with Card and Table based on JSON Data and custom templates for Cards. This is Designed to use with [Bootstrap 3](http://getbootstrap.com) or more

![Screenshot](https://github.com/ashutosh456/CardGrid/blob/master/ScreenShots/cardgrid_Employee_ScreenShot.png)
![Screenshot](https://github.com/ashutosh456/CardGrid/blob/master/ScreenShots/cardgrid_Employee_TableView.png)

## Help In Project
Currently Project is deployed with ASP.NET MVC Project. This plugin is written in TypeScript.
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
## Options
### Grid
    url: string;                                // URL To get Data
    type: string;                               // GET/POST
    data?: any;                                 // Optional : JSON Data
    pagination?: boolean;                       // Optional : Pagination True/False
    searchBox: boolean;                         // Optional : Show Search Box true/false
    buttons?: Array<Button>;                    // Optional : [Buttons] Buttons to display
    success?: any;                              // Optional : function(data){ }
    recordsPerPage?: Array<number>;             // Optional : [Numbers] Default : 12
    template?: (data: any) => HTMLDivElement;   // Optional : function(data) {} : Cards Template 
    cols?: Array<column>;                       // Optional : [Column] : Columns for Table View
    currentView?: string;                       // Optional : 'Get and Set Default View - card/table; Default is 'çard'
    itemsPerRow: number;                        // Optional : Itemps per Row; Default : 4
    sortCols?: Array<SortColumn>;               // Optional : [SortColumn] : List of Columns for Sorting; Default: Id
    multiSelect?: boolean;                      // Optional : Allow MultiSelect; Default: true
    afterGridRender?: () => void;               // Optional : function() { }  Execute after Rendering Grid
    queryParams?: any;                          // Optional : JSON Array: To Set Query Params using querystring
    beforeSend?: any;                           // Optional : function() { }: to perform addional before sending request for Data. 
    beforeGridLoad?: (data: any) => any;        // Optional : function(data) { } : Peform Opertion before Loading grid. 
### Pagination Parameters 
    total: number;                              // Total No of Records Available
    noOfPages: number;                          // No of Pages
    rows: number;                               // Size of Page / No of Items per Page
    page: number;                               // Current Selected Page
    searchParam?: string;                       // Search Text
    sidx?: string;                              // Sorting Field; Default- first sort column
    sord?: string                               // Sorting Direction - asc/desc
### Columns (Only Application in Table View)
    id: string;                                 // Column Name from Data
    hidden?: boolean;                           // Optional : Visible/Hidden ; Default: false
    sortable?: boolean;                         // ### Not Implemented Now
    title: string;                              // Title to display in Column View
    template?: (data: any) => HTMLDivElement;   // function(data){ } : to define custom logic or template
    width?: number;                             // width
### Sort Columns
    text: string;                               // Text to Display ; Exp: "Date Of Birth"
    value: string;                              // Value   ; Exp: "dob"
### Buttons
    text: string;                               // Text to Display in Button
    className?: string;                         // css ClassName 
    id?: string;                                // Button Id
    enable?: boolean;                           // Enable/Disable Button, Default : true
    hidden?: boolean;                           // Default : false
    onClick: (data: any) => void;               // function(data) { }; onclick event
    buttons?: Array<DropDownButton>;            // [DropDownButtons] Optional: is to add DropDown Buttons 
### Drop Down Buttons
    text: string;                               // Text to Display in Button   
    id?: string;                                // Button Id
    enable?: boolean;                           // Enable/Disable Button, Default : true
    hidden?: boolean;                           // Default : false
    onClick: (data: any) => void;               // function(data) { }; onclick event
    
    
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
 * Add Javascript Libraries  
     ``` 
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
