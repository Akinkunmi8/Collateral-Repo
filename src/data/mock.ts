import {WorkItem,User} from '../types';
export const items:WorkItem[]=[
{id:'1',reference:'COL-2026-00128',customer:'Apex Manufacturing Ltd',asset:'Commercial Property',status:'Pending',loanStatus:'Active',tenure:'36 months',endDate:'2028-09-15'},
{id:'2',reference:'COL-2026-00127',customer:'Meridian Logistics',asset:'Fleet / Vehicles',status:'Approved',loanStatus:'Inactive',tenure:'24 months',endDate:'2026-08-31'},
{id:'3',reference:'COL-2026-00126',customer:'Northstar Foods Plc',asset:'Plant & Machinery',status:'Rejected',loanStatus:'Active',tenure:'48 months',endDate:'2029-01-20'},
{id:'4',reference:'COL-2026-00125',customer:'BlueRock Energy',asset:'Residential Property',status:'Archived',loanStatus:'Inactive',tenure:'18 months',endDate:'2026-06-10'}];
export const users:User[]=[
{id:'1',name:'Ada Okafor',email:'ada.okafor@bank.com',role:'Collateral Officer',status:'Active'},
{id:'2',name:'Tunde Bello',email:'tunde.bello@bank.com',role:'Valuer',status:'Active'},
{id:'3',name:'Grace Mensah',email:'grace.mensah@bank.com',role:'Legal Officer',status:'Inactive'}];