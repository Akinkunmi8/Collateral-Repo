export type Status='Pending'|'Approved'|'Rejected'|'Archived'|'Released'|'Active'|'Inactive';
export interface WorkItem {id:string;reference:string;customer:string;asset:string;status:Status;loanStatus?:'Active'|'Inactive';tenure?:string;endDate?:string}
export interface User {id:string;name:string;email:string;role:string;status:'Active'|'Inactive'}