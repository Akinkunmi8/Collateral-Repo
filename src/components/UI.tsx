import {ReactNode} from 'react';import {Search} from 'lucide-react';import {WorkItem} from '../types';
export function PageTitle({title,subtitle,action}:{title:string;subtitle:string;action?:ReactNode}){return <div className="pageTitle"><div><h1>{title}</h1><p>{subtitle}</p></div>{action}</div>}
export function Stat({
    label,
    value,
    kind = "blue",
    onClick,
    active = false,
    actionText = "View records →"
}: {
    label: string;
    value: number | string;
    kind?: string;
    onClick?: () => void;
    active?: boolean;
    actionText?: string;
}) {
    return (
        <button
            type="button"
            className={`stat ${kind} ${active ? "active" : ""}`}
            onClick={onClick}
        >
            <span>{label}</span>

            <strong>{value}</strong>

            {actionText && (
                <small>{actionText}</small>
            )}
        </button>
    );
}
export function SearchBox({placeholder='Search reference, customer or asset...'}:{placeholder?:string}){return <div className="searchBox"><Search size={17}/><input placeholder={placeholder}/></div>}
export function WorkTable({rows,mode='view'}:{rows:WorkItem[];mode?:'verify'|'review'|'release'|'view'}){return <div className="tableWrap"><table><thead><tr><th>Reference</th><th>Customer</th><th>Asset</th><th>Status</th>{mode==='release'&&<><th>Loan Status</th><th>End Date</th></>}<th>Action</th></tr></thead><tbody>{rows.map(r=><tr key={r.id}><td><b>{r.reference}</b></td><td>{r.customer}</td><td>{r.asset}</td><td><span className={`badge ${r.status.toLowerCase()}`}>{r.status}</span></td>{mode==='release'&&<><td><span className={`badge ${r.loanStatus?.toLowerCase()}`}>{r.loanStatus}</span></td><td>{r.endDate}</td></>}<td><button className="linkBtn" disabled={mode==='release'&&r.loanStatus==='Active'}>{mode==='verify'&&r.status==='Pending'?'Verify':mode==='review'&&r.status==='Rejected'?'Re-validate':mode==='release'?'Release':'View more'}</button></td></tr>)}</tbody></table></div>}